using MatrixRoomUtils.Core;
using MatrixRoomUtils.Core.Responses;
using MatrixRoomUtils.Core.Services;
using Microsoft.AspNetCore.Components;

namespace MatrixRoomUtils.Web.Classes;

public class MRUStorageWrapper {
    private readonly TieredStorageService _storageService;
    private readonly HomeserverProviderService _homeserverProviderService;
    private readonly NavigationManager _navigationManager;

    public MRUStorageWrapper(
        TieredStorageService storageService,
        HomeserverProviderService homeserverProviderService,
        NavigationManager navigationManager
    ) {
        _storageService = storageService;
        _homeserverProviderService = homeserverProviderService;
        _navigationManager = navigationManager;
    }

    public async Task<List<LoginResponse>?> GetAllTokens() {
        return await _storageService.DataStorageProvider.LoadObjectAsync<List<LoginResponse>>("mru.tokens") ??
               new List<LoginResponse>();
    }

    public async Task<LoginResponse?> GetCurrentToken() {
        var currentToken = await _storageService.DataStorageProvider.LoadObjectAsync<LoginResponse>("token");
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

    public async Task AddToken(LoginResponse loginResponse) {
        var tokens = await GetAllTokens();
        if (tokens == null) {
            tokens = new List<LoginResponse>();
        }

        tokens.Add(loginResponse);
        await _storageService.DataStorageProvider.SaveObjectAsync("mru.tokens", tokens);
    }

    private async Task<AuthenticatedHomeServer?> GetCurrentSession() {
        var token = await GetCurrentToken();
        if (token == null) {
            return null;
        }

        return await _homeserverProviderService.GetAuthenticatedWithToken(token.Homeserver, token.AccessToken);
    }

    public async Task<AuthenticatedHomeServer?> GetCurrentSessionOrNavigate() {
        AuthenticatedHomeServer? session = null;

        try {
            //catch if the token is invalid
            session = await GetCurrentSession();
        }
        catch (MatrixException e) {
            if (e.ErrorCode == "M_UNKNOWN_TOKEN") {
                var token = await GetCurrentToken();
                _navigationManager.NavigateTo("/InvalidSession?ctx=" + token.AccessToken);
                return null;
            }

            throw;
        }

        if (session is null) {
            _navigationManager.NavigateTo("/Login");
        }

        return session;
    }

    public class Settings {
        public DeveloperSettings DeveloperSettings { get; set; } = new();
    }

    public class DeveloperSettings {
        public bool EnableLogViewers { get; set; } = false;
        public bool EnableConsoleLogging { get; set; } = true;
        public bool EnablePortableDevtools { get; set; } = false;
    }

    public async Task RemoveToken(LoginResponse auth) {
        var tokens = await GetAllTokens();
        if (tokens == null) {
            return;
        }

        tokens.RemoveAll(x => x.AccessToken == auth.AccessToken);
        await _storageService.DataStorageProvider.SaveObjectAsync("mru.tokens", tokens);
    }

    public async Task SetCurrentToken(LoginResponse? auth) {
        _storageService.DataStorageProvider.SaveObjectAsync("token", auth);
    }
}
