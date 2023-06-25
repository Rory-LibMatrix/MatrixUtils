namespace MatrixRoomUtils.Core.Services;

public class HomeserverProviderService {
    private readonly TieredStorageService _tieredStorageService;

    public HomeserverProviderService(TieredStorageService tieredStorageService) {
        Console.WriteLine("Homeserver provider service instantiated!");
        _tieredStorageService = tieredStorageService;
        Console.WriteLine(
            $"New HomeserverProviderService created with TieredStorageService<{string.Join(", ", tieredStorageService.GetType().GetProperties().Select(x => x.Name))}>!");
    }

    public async Task<AuthenticatedHomeServer> GetAuthenticatedWithToken(string homeserver, string accessToken) {
        return await new AuthenticatedHomeServer(_tieredStorageService, homeserver, accessToken).Configure();
    }
}