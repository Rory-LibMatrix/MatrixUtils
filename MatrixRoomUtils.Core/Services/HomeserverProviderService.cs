using System.Net.Http.Headers;
using System.Net.Http.Json;
using MatrixRoomUtils.Core.Extensions;
using Microsoft.Extensions.Logging;
namespace MatrixRoomUtils.Core.Services;

public class HomeserverProviderService {
    private readonly TieredStorageService _tieredStorageService;
    private readonly ILogger<HomeserverProviderService> _logger;
    private readonly HomeserverResolverService _homeserverResolverService;

    public HomeserverProviderService(TieredStorageService tieredStorageService, ILogger<HomeserverProviderService> logger, HomeserverResolverService homeserverResolverService) {
        Console.WriteLine("Homeserver provider service instantiated!");
        _tieredStorageService = tieredStorageService;
        _logger = logger;
        _homeserverResolverService = homeserverResolverService;
        logger.LogDebug(
            $"New HomeserverProviderService created with TieredStorageService<{string.Join(", ", tieredStorageService.GetType().GetProperties().Select(x => x.Name))}>!");
    }

    public async Task<AuthenticatedHomeServer> GetAuthenticatedWithToken(string homeserver, string accessToken) {
        var hs = new AuthenticatedHomeServer(_tieredStorageService, homeserver, accessToken);
        hs.FullHomeServerDomain = await _homeserverResolverService.ResolveHomeserverFromWellKnown(homeserver);
        hs._httpClient.Dispose();
        hs._httpClient = new MatrixHttpClient { BaseAddress = new Uri(hs.FullHomeServerDomain) };
        hs._httpClient.Timeout = TimeSpan.FromSeconds(5);
        hs._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        hs.WhoAmI = (await hs._httpClient.GetFromJsonAsync<WhoAmIResponse>("/_matrix/client/v3/account/whoami"))!;
        return hs;
    }
}