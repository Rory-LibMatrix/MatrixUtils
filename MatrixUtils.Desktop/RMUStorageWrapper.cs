using Avalonia;
using LibMatrix;
using LibMatrix.Homeservers;
using LibMatrix.Responses;
using LibMatrix.Services;

namespace MatrixUtils.Desktop;

public class RMUStorageWrapper(TieredStorageService storageService, HomeserverProviderService homeserverProviderService) {
    public async Task<List<LoginResponse>?> GetAllTokens() {
        if (!await storageService.DataStorageProvider.ObjectExistsAsync("rmu.tokens")) {
            return null;
        }
        return await storageService.DataStorageProvider.LoadObjectAsync<List<LoginResponse>>("rmu.tokens") ??
               new List<LoginResponse>();
    }

    public async Task<LoginResponse?> GetCurrentToken() {
        if (!await storageService.DataStorageProvider.ObjectExistsAsync("token")) {
            return null;
        }
        var currentToken = await storageService.DataStorageProvider.LoadObjectAsync<LoginResponse>("token");
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
        var tokens = await GetAllTokens() ?? new List<LoginResponse>();

        tokens.Add(loginResponse);
        await storageService.DataStorageProvider.SaveObjectAsync("rmu.tokens", tokens);
        if (await GetCurrentToken() is null)
            await SetCurrentToken(loginResponse);
    }

    private async Task<AuthenticatedHomeserverGeneric?> GetCurrentSession() {
        var token = await GetCurrentToken();
        if (token == null) {
            return null;
        }

        return await homeserverProviderService.GetAuthenticatedWithToken(token.Homeserver, token.AccessToken);
    }

    public async Task<AuthenticatedHomeserverGeneric?> GetCurrentSessionOrPrompt() {
        AuthenticatedHomeserverGeneric? session = null;

        try {
            //catch if the token is invalid
            session = await GetCurrentSession();
        }
        catch (MatrixException e) {
            if (e.ErrorCode == "M_UNKNOWN_TOKEN") {
                var token = await GetCurrentToken();
                // _navigationManager.NavigateTo("/InvalidSession?ctx=" + token.AccessToken);
                return null;
            }

            throw;
        }

        if (session is null) {
            // _navigationManager.NavigateTo("/Login");
            var wnd = new LoginWindow(this);
            wnd.Position = MainWindow.Instance.Position + new PixelPoint(50, 50);
            await wnd.ShowDialog(MainWindow.Instance);
            while (wnd.IsVisible) {
                await Task.Delay(100);
            }
            session = await GetCurrentSession();
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
        await storageService.DataStorageProvider.SaveObjectAsync("rmu.tokens", tokens);
    }

    public async Task SetCurrentToken(LoginResponse? auth) => await storageService.DataStorageProvider.SaveObjectAsync("token", auth);

    public async Task<LoginResponse?> Login(string homeserver, string username, string password) {
        try {
            return await homeserverProviderService.Login(homeserver, username, password);
        }
        catch (MatrixException e) {
            if (e.ErrorCode == "M_FORBIDDEN") {
                return null;
            }

            throw;
        }
    }
}
