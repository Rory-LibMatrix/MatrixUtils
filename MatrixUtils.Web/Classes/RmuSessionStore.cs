using LibMatrix;
using LibMatrix.Homeservers;
using LibMatrix.Services;
using Microsoft.AspNetCore.Components;

namespace MatrixUtils.Web.Classes;

public class RmuSessionStore(
    ILogger<RmuSessionStore> logger,
    TieredStorageService storageService,
    HomeserverProviderService homeserverProviderService,
    NavigationManager navigationManager) {
    private SessionInfo? CurrentSession { get; set; }
    private Dictionary<string, SessionInfo> SessionCache { get; set; } = [];

    private bool _isInitialized;
    private static readonly SemaphoreSlim InitSemaphore = new(1, 1);

#region Sessions

    public async Task<Dictionary<string, SessionInfo>> GetAllSessions() {
        await LoadStorage();
        logger.LogTrace("Getting all tokens.");
        return SessionCache;
    }

    public async Task<SessionInfo?> GetSession(string sessionId) {
        await LoadStorage();
        if (string.IsNullOrEmpty(sessionId)) {
            logger.LogWarning("No session ID provided.");
            return null;
        }

        if (SessionCache.TryGetValue(sessionId, out var cachedSession))
            return cachedSession;

        logger.LogWarning("Session {sessionId} not found in all tokens.", sessionId);
        return null;
    }

    public async Task<SessionInfo?> GetCurrentSession(bool log = true) {
        await LoadStorage();
        if (log) logger.LogTrace("Getting current token.");
        if (CurrentSession is not null) return CurrentSession;

        var currentSessionId = await storageService.DataStorageProvider!.LoadObjectAsync<string>("rmu.session");
        if (currentSessionId == null) {
            if (log) logger.LogWarning("No current session ID found in storage.");
            return null;
        }

        return await GetSession(currentSessionId);
    }

    public async Task<string> AddSession(UserAuth auth) {
        await LoadStorage();
        logger.LogTrace("Adding token.");

        var sessionId = auth.GetHashCode().ToString();
        SessionCache[sessionId] = new() {
            Auth = auth,
            SessionId = sessionId
        };

        await SaveStorage();
        if (CurrentSession == null) await SetCurrentSession(sessionId);

        return sessionId;
    }

    public async Task RemoveSession(string sessionId) {
        await LoadStorage();
        if (SessionCache.Count == 0) {
            logger.LogWarning("No sessions found.");
            return;
        }

        logger.LogTrace("Removing session {sessionId}.", sessionId);

        if ((await GetCurrentSession())?.SessionId == sessionId)
            await SetCurrentSession(SessionCache.FirstOrDefault(x => x.Key != sessionId).Key);

        if (SessionCache.Remove(sessionId)) {
            logger.LogInformation("RemoveSession: Removed session {sessionId}.", sessionId);
            logger.LogInformation("RemoveSession: Remaining sessions: {sessionIds}.", string.Join(", ", SessionCache.Keys));
            await SaveStorage(log: true);
        }
        else
            logger.LogWarning("RemoveSession: Session {sessionId} not found.", sessionId);
    }

    public async Task SetCurrentSession(string? sessionId) {
        await LoadStorage();
        logger.LogTrace("Setting current session to {sessionId}.", sessionId);
        CurrentSession = await GetSession(sessionId);
        await SaveStorage();
    }

#endregion

#region Homeservers

    public async Task<AuthenticatedHomeserverGeneric?> GetHomeserver(string session, bool log = true) {
        await LoadStorage();
        if (log) logger.LogTrace("Getting session.");
        if (!SessionCache.TryGetValue(session, out var cachedSession)) return null;
        if (cachedSession.Homeserver is not null) return cachedSession.Homeserver;

        try {
            cachedSession.Homeserver =
                await homeserverProviderService.GetAuthenticatedWithToken(cachedSession.Auth.Homeserver, cachedSession.Auth.AccessToken, cachedSession.Auth.Proxy);
        }
        catch (Exception e) {
            logger.LogError("Failed to get info for {0} via {1}: {2}", cachedSession.Auth.UserId, cachedSession.Auth.Homeserver, e);
            logger.LogError("Continuing with server-less session");
            cachedSession.Homeserver = await homeserverProviderService.GetAuthenticatedWithToken(cachedSession.Auth.Homeserver, cachedSession.Auth.AccessToken,
                cachedSession.Auth.Proxy, useGeneric: true, enableServer: false);
        }

        return cachedSession.Homeserver;
    }

    public async Task<AuthenticatedHomeserverGeneric?> GetCurrentHomeserver(bool log = true, bool navigateOnFailure = false) {
        await LoadStorage();
        if (log) logger.LogTrace("Getting current session.");
        if (CurrentSession?.Homeserver is not null) return CurrentSession.Homeserver;

        var currentSession = CurrentSession ??= await GetCurrentSession(log: false);
        if (currentSession == null) {
            if (navigateOnFailure) {
                logger.LogInformation("No session found. Navigating to login.");
                navigationManager.NavigateTo("/Login");
            }

            return null;
        }

        try {
            return currentSession.Homeserver ??= await GetHomeserver(currentSession.SessionId);
        }
        catch (MatrixException e) {
            if (e.ErrorCode == "M_UNKNOWN_TOKEN" && navigateOnFailure) {
                logger.LogWarning("Encountered invalid token for {user} on {homeserver}", currentSession.Auth.UserId, currentSession.Auth.Homeserver);
                if (navigateOnFailure) {
                    navigationManager.NavigateTo("/InvalidSession?ctx=" + currentSession.SessionId);
                }
            }

            throw;
        }
    }

    public async IAsyncEnumerable<AuthenticatedHomeserverGeneric> TryGetAllHomeservers(bool log = true, bool ignoreFailures = true) {
        await LoadStorage();
        if (log) logger.LogTrace("Getting all homeservers.");
        var tasks = SessionCache.Values.Select(async session => {
            if (ignoreFailures && session.Auth.LastFailureReason != null && session.Auth.LastFailureReason != UserAuth.FailureReason.None) {
                if (log) logger.LogTrace("Skipping session {sessionId} due to previous failure: {reason}", session.SessionId, session.Auth.LastFailureReason);
                return null;
            }

            try {
                var hs = await GetHomeserver(session.SessionId, log: false);
                if (session.Auth.LastFailureReason != null) {
                    SessionCache[session.SessionId].Auth.LastFailureReason = null;
                    await SaveStorage();
                }

                return hs;
            }
            catch (Exception e) {
                logger.LogError("TryGetAllHomeservers: Failed to get homeserver for {userId} via {homeserver}: {ex}", session.Auth.UserId, session.Auth.Homeserver, e);
                var reason = SessionCache[session.SessionId].Auth.LastFailureReason = e switch {
                    MatrixException { ErrorCode: MatrixException.ErrorCodes.M_UNKNOWN_TOKEN } => UserAuth.FailureReason.InvalidToken,
                    HttpRequestException => UserAuth.FailureReason.NetworkError,
                    _ => UserAuth.FailureReason.UnknownError
                };
                await SaveStorage(log: true);

                // await LoadStorage(true);
                if (SessionCache[session.SessionId].Auth.LastFailureReason != reason) {
                    await Console.Error.WriteLineAsync(
                        $"Warning: Session {session.SessionId} failure reason changed during reload from {reason} to {SessionCache[session.SessionId].Auth.LastFailureReason}");
                }

                throw;
            }
        }).ToList();

        while (tasks.Count != 0) {
            var finished = await Task.WhenAny(tasks);
            tasks.Remove(finished);
            if (finished.IsFaulted) continue;

            var result = await finished;
            if (result != null) yield return result;
        }
    }

#endregion

#region Storage

    private async Task LoadStorage(bool hasMigrated = false) {
        if (!await storageService.DataStorageProvider!.ObjectExistsAsync("rmu.sessions") || !await storageService.DataStorageProvider.ObjectExistsAsync("rmu.session")) {
            if (!hasMigrated) {
                await RunMigrations();
                await LoadStorage(true);
            }
            else
                logger.LogWarning("No sessions found in storage.");

            return;
        }

        SessionCache = (await storageService.DataStorageProvider.LoadObjectAsync<Dictionary<string, UserAuth>>("rmu.sessions") ?? throw new Exception("Failed to load sessions"))
            .ToDictionary(x => x.Key, x => new SessionInfo {
                SessionId = x.Key,
                Auth = x.Value
            });

        var currentSessionId = await storageService.DataStorageProvider.LoadObjectAsync<string>("rmu.session");
        if (currentSessionId == null) {
            logger.LogWarning("No current session found in storage.");
            return;
        }

        if (!SessionCache.TryGetValue(currentSessionId, out var currentSession)) {
            logger.LogWarning("Current session {currentSessionId} not found in storage.", currentSessionId);
            return;
        }

        CurrentSession = currentSession;
    }

    private async Task SaveStorage(bool log = false) {
        if (log) logger.LogWarning("Saving {count} sessions to storage.", SessionCache.Count);
        await storageService.DataStorageProvider!.SaveObjectAsync("rmu.sessions",
            SessionCache.ToDictionary(
                x => x.Key,
                x => x.Value.Auth
            )
        );
        await storageService.DataStorageProvider.SaveObjectAsync("rmu.session", CurrentSession?.SessionId);
        if (log) logger.LogWarning("{count} sessions saved to storage.", SessionCache.Count);
    }

#endregion

#region Migrations

    public async Task RunMigrations() {
        await MigrateFromMru();
        await MigrateAccountsToKeyedStorage();
    }

    private async Task MigrateFromMru() {
        var dsp = storageService.DataStorageProvider!;
        if (await dsp.ObjectExistsAsync("token") || await dsp.ObjectExistsAsync("tokens")) {
            logger.LogInformation("Migrating from unnamespaced localstorage!");
            if (await dsp.ObjectExistsAsync("token")) {
                var oldToken = await dsp.LoadObjectAsync<UserAuth>("token");
                if (oldToken != null) {
                    await dsp.SaveObjectAsync("mru.token", oldToken);
                    await dsp.DeleteObjectAsync("token");
                }
            }

            if (await dsp.ObjectExistsAsync("tokens")) {
                var oldTokens = await dsp.LoadObjectAsync<List<UserAuth>>("tokens");
                if (oldTokens != null) {
                    await dsp.SaveObjectAsync("mru.tokens", oldTokens);
                    await dsp.DeleteObjectAsync("tokens");
                }
            }
        }

        if (await dsp.ObjectExistsAsync("mru.token") || await dsp.ObjectExistsAsync("mru.tokens")) {
            logger.LogInformation("Migrating from MRU token namespace!");
            if (await dsp.ObjectExistsAsync("mru.token")) {
                var oldToken = await dsp.LoadObjectAsync<UserAuth>("mru.token");
                if (oldToken != null) {
                    await dsp.SaveObjectAsync("rmu.token", oldToken);
                    await dsp.DeleteObjectAsync("mru.token");
                }
            }

            if (await dsp.ObjectExistsAsync("mru.tokens")) {
                var oldTokens = await dsp.LoadObjectAsync<List<UserAuth>>("mru.tokens");
                if (oldTokens != null) {
                    await dsp.SaveObjectAsync("rmu.tokens", oldTokens);
                    await dsp.DeleteObjectAsync("mru.tokens");
                }
            }
        }
    }

    private async Task MigrateAccountsToKeyedStorage() {
        var dsp = storageService.DataStorageProvider!;
        if (!await dsp.ObjectExistsAsync("rmu.tokens")) return;
        logger.LogInformation("Migrating accounts to keyed storage!");
        var tokens = await dsp.LoadObjectAsync<UserAuth[]>("rmu.tokens") ?? throw new Exception("Failed to load tokens");
        Dictionary<string, UserAuth> keyedTokens = tokens.ToDictionary(x => x.GetHashCode().ToString(), x => x);

        if (await dsp.ObjectExistsAsync("rmu.token")) {
            var token = await dsp.LoadObjectAsync<UserAuth>("rmu.token") ?? throw new Exception("Failed to load token");
            var sessionId = keyedTokens.FirstOrDefault(x => x.Value.Equals(token)).Key;

            if (sessionId is null) keyedTokens.Add(sessionId = token.GetHashCode().ToString(), token);
            await dsp.SaveObjectAsync("rmu.session", sessionId);

            await dsp.DeleteObjectAsync("rmu.token");
        }

        await dsp.SaveObjectAsync("rmu.sessions", keyedTokens);
        await dsp.DeleteObjectAsync("rmu.tokens");
    }

#endregion

    public class Settings {
        public DeveloperSettings DeveloperSettings { get; set; } = new();
    }

    public class DeveloperSettings {
        public bool EnableLogViewers { get; set; }
        public bool EnableConsoleLogging { get; set; } = true;
        public bool EnablePortableDevtools { get; set; }
    }

    public class SessionInfo {
        public required string SessionId { get; set; }
        public required UserAuth Auth { get; set; }
        public AuthenticatedHomeserverGeneric? Homeserver { get; set; }
    }
}