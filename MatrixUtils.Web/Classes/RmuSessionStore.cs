using System.Text.Json;
using System.Text.Json.Nodes;
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

        if (CurrentSession == null) await SetCurrentSession(sessionId);
        else await SaveStorage();

        return sessionId;
    }

    public async Task RemoveSession(string sessionId) {
        await LoadStorage();
        logger.LogTrace("Removing session {sessionId}.", sessionId);
        var tokens = await GetAllSessions();
        if (tokens == null) {
            return;
        }

        if ((await GetCurrentSession())?.SessionId == sessionId)
            await SetCurrentSession(tokens.First(x => x.Key != sessionId).Key);

        if (tokens.Remove(sessionId))
            await SaveStorage();
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

    private async Task SaveStorage() {
        await storageService.DataStorageProvider!.SaveObjectAsync("rmu.sessions",
            SessionCache.ToDictionary(
                x => x.Key,
                x => x.Value.Auth
            )
        );
        await storageService.DataStorageProvider.SaveObjectAsync("rmu.session", CurrentSession?.SessionId);
    }

#endregion

#region Migrations

    public async Task RunMigrations() {
        await MigrateFromMru();
        await MigrateAccountsToKeyedStorage();
    }

    private async Task MigrateFromMru() {
        logger.LogInformation("Migrating from MRU token namespace!");
        var dsp = storageService.DataStorageProvider!;
        if (await dsp.ObjectExistsAsync("token")) {
            var oldToken = await dsp.LoadObjectAsync<UserAuth>("token");
            if (oldToken != null) {
                await dsp.SaveObjectAsync("rmu.token", oldToken);
                await dsp.DeleteObjectAsync("token");
            }
        }

        if (await dsp.ObjectExistsAsync("tokens")) {
            var oldTokens = await dsp.LoadObjectAsync<List<UserAuth>>("tokens");
            if (oldTokens != null) {
                await dsp.SaveObjectAsync("rmu.tokens", oldTokens);
                await dsp.DeleteObjectAsync("tokens");
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