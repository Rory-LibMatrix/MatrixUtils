using LibMatrix;
using LibMatrix.Homeservers;
using LibMatrix.Services;
using Microsoft.AspNetCore.Components;

namespace MatrixUtils.Web.Classes;

public class RMUStorageWrapper(ILogger<RMUStorageWrapper> logger, TieredStorageService storageService, HomeserverProviderService homeserverProviderService, NavigationManager navigationManager) {
    public async Task<List<UserAuth>?> GetAllTokens() {
        logger.LogTrace("Getting all tokens.");
        return await storageService.DataStorageProvider.LoadObjectAsync<List<UserAuth>>("rmu.tokens") ??
               new List<UserAuth>();
    }

    public async Task<UserAuth?> GetCurrentToken() {
        logger.LogTrace("Getting current token.");
        var currentToken = await storageService.DataStorageProvider.LoadObjectAsync<UserAuth>("rmu.token");
        var allTokens = await GetAllTokens();
        if (allTokens is null or { Count: 0 }) {
            await SetCurrentToken(null);
            return null;
        }

        if (currentToken is null) {
            await SetCurrentToken(currentToken = allTokens[0]);
        }

        if (!allTokens.Any(x => x.AccessToken == currentToken.AccessToken)) {
            await SetCurrentToken(currentToken = allTokens[0]);
        }

        return currentToken;
    }

    public async Task AddToken(UserAuth UserAuth) {
        logger.LogTrace("Adding token.");
        var tokens = await GetAllTokens() ?? new List<UserAuth>();

        tokens.Add(UserAuth);
        await storageService.DataStorageProvider.SaveObjectAsync("rmu.tokens", tokens);
    }

    private async Task<AuthenticatedHomeserverGeneric?> GetCurrentSession() {
        logger.LogTrace("Getting current session.");
        var token = await GetCurrentToken();
        if (token == null) {
            return null;
        }

        return await homeserverProviderService.GetAuthenticatedWithToken(token.Homeserver, token.AccessToken, token.Proxy);
    }

    public async Task<AuthenticatedHomeserverGeneric?> GetSession(UserAuth userAuth) {
        logger.LogTrace("Getting session.");
        return await homeserverProviderService.GetAuthenticatedWithToken(userAuth.Homeserver, userAuth.AccessToken, userAuth.Proxy);
    }

    public async Task<AuthenticatedHomeserverGeneric?> GetCurrentSessionOrNavigate() {
        logger.LogTrace("Getting current session or navigating.");
        AuthenticatedHomeserverGeneric? session = null;

        try {
            //catch if the token is invalid
            session = await GetCurrentSession();
        }
        catch (MatrixException e) {
            if (e.ErrorCode == "M_UNKNOWN_TOKEN") {
                var token = await GetCurrentToken();
                logger.LogWarning("Encountered invalid token for {user} on {homeserver}", token.UserId, token.Homeserver);
                navigationManager.NavigateTo("/InvalidSession?ctx=" + token.AccessToken);
                return null;
            }

            throw;
        }

        if (session is null) {
            logger.LogInformation("No session found. Navigating to login.");
            navigationManager.NavigateTo("/Login");
        }

        return session;
    }

    public class Settings {
        public DeveloperSettings DeveloperSettings { get; set; } = new();
    }

    public class DeveloperSettings {
        public bool EnableLogViewers { get; set; }
        public bool EnableConsoleLogging { get; set; } = true;
        public bool EnablePortableDevtools { get; set; }
    }

    public async Task RemoveToken(UserAuth auth) {
        logger.LogTrace("Removing token.");
        var tokens = await GetAllTokens();
        if (tokens == null) {
            return;
        }

        tokens.RemoveAll(x => x.AccessToken == auth.AccessToken);
        await storageService.DataStorageProvider.SaveObjectAsync("rmu.tokens", tokens);
    }

    public async Task SetCurrentToken(UserAuth? auth) {
        logger.LogTrace("Setting current token.");
        await storageService.DataStorageProvider.SaveObjectAsync("rmu.token", auth);
    }

    public async Task MigrateFromMRU() {
        logger.LogInformation("Migrating from MRU token namespace!");
        var dsp = storageService.DataStorageProvider!;
        if(await dsp.ObjectExistsAsync("token")) {
            var oldToken = await dsp.LoadObjectAsync<UserAuth>("token");
            if (oldToken != null) {
                await dsp.SaveObjectAsync("rmu.token", oldToken);
                await dsp.DeleteObjectAsync("tokens");
            }
        }
        
        if(await dsp.ObjectExistsAsync("tokens")) {
            var oldTokens = await dsp.LoadObjectAsync<List<UserAuth>>("tokens");
            if (oldTokens != null) {
                await dsp.SaveObjectAsync("rmu.tokens", oldTokens);
                await dsp.DeleteObjectAsync("tokens");
            }
        }
        
        if(await dsp.ObjectExistsAsync("mru.tokens")) {
            var oldTokens = await dsp.LoadObjectAsync<List<UserAuth>>("mru.tokens");
            if (oldTokens != null) {
                await dsp.SaveObjectAsync("rmu.tokens", oldTokens);
                await dsp.DeleteObjectAsync("mru.tokens");
            }
        }
    }
}
