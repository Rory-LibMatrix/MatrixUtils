using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Responses;
using Microsoft.Extensions.Logging;

namespace MatrixRoomUtils.Core.Services;

public class HomeserverProviderService {
    private readonly TieredStorageService _tieredStorageService;
    private readonly ILogger<HomeserverProviderService> _logger;
    private readonly HomeserverResolverService _homeserverResolverService;

    public HomeserverProviderService(TieredStorageService tieredStorageService,
        ILogger<HomeserverProviderService> logger, HomeserverResolverService homeserverResolverService) {
        Console.WriteLine("Homeserver provider service instantiated!");
        _tieredStorageService = tieredStorageService;
        _logger = logger;
        _homeserverResolverService = homeserverResolverService;
        logger.LogDebug(
            $"New HomeserverProviderService created with TieredStorageService<{string.Join(", ", tieredStorageService.GetType().GetProperties().Select(x => x.Name))}>!");
    }

    public async Task<AuthenticatedHomeServer> GetAuthenticatedWithToken(string homeserver, string accessToken,
        string? overrideFullDomain = null) {
        var hs = new AuthenticatedHomeServer(_tieredStorageService, homeserver, accessToken);
        hs.FullHomeServerDomain = overrideFullDomain ??
                                  await _homeserverResolverService.ResolveHomeserverFromWellKnown(homeserver);
        hs._httpClient.Dispose();
        hs._httpClient = new MatrixHttpClient { BaseAddress = new Uri(hs.FullHomeServerDomain) };
        hs._httpClient.Timeout = TimeSpan.FromSeconds(120);
        hs._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        hs.WhoAmI = (await hs._httpClient.GetFromJsonAsync<WhoAmIResponse>("/_matrix/client/v3/account/whoami"))!;
        return hs;
    }

    public async Task<RemoteHomeServer> GetRemoteHomeserver(string homeserver, string? overrideFullDomain = null) {
        var hs = new RemoteHomeServer(homeserver);
        hs.FullHomeServerDomain = overrideFullDomain ??
                                  await _homeserverResolverService.ResolveHomeserverFromWellKnown(homeserver);
        hs._httpClient.Dispose();
        hs._httpClient = new MatrixHttpClient { BaseAddress = new Uri(hs.FullHomeServerDomain) };
        hs._httpClient.Timeout = TimeSpan.FromSeconds(120);
        return hs;
    }

    public async Task<LoginResponse> Login(string homeserver, string user, string password,
        string? overrideFullDomain = null) {
        var hs = await GetRemoteHomeserver(homeserver, overrideFullDomain);
        var payload = new LoginRequest {
            Identifier = new() { User = user },
            Password = password
        };
        var resp = await hs._httpClient.PostAsJsonAsync("/_matrix/client/v3/login", payload);
        var data = await resp.Content.ReadFromJsonAsync<LoginResponse>();
        return data!;
    }

    private class LoginRequest {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "m.login.password";

        [JsonPropertyName("identifier")]
        public LoginIdentifier Identifier { get; set; } = new();

        [JsonPropertyName("password")]
        public string Password { get; set; } = "";

        [JsonPropertyName("initial_device_display_name")]
        public string InitialDeviceDisplayName { get; set; } = "Rory&::LibMatrix";

        public class LoginIdentifier {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "m.id.user";

            [JsonPropertyName("user")]
            public string User { get; set; } = "";
        }
    }
}