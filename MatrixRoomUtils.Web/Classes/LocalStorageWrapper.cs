using Blazored.LocalStorage;
using MatrixRoomUtils.Authentication;
using MatrixRoomUtils.Responses;

namespace MatrixRoomUtils.Web.Classes;

public partial class LocalStorageWrapper
{
    //some basic logic
    public static async Task LoadFromLocalStorage(ILocalStorageService localStorage)
    {
        RuntimeCache.AccessToken = await localStorage.GetItemAsync<string>("rory.matrixroomutils.token");
        RuntimeCache.CurrentHomeserver = await localStorage.GetItemAsync<string>("rory.matrixroomutils.current_homeserver");
        RuntimeCache.LoginSessions = await localStorage.GetItemAsync<Dictionary<string, UserInfo>>("rory.matrixroomutils.user_cache") ?? new();
        RuntimeCache.HomeserverResolutionCache = await localStorage.GetItemAsync<Dictionary<string, HomeServerResolutionResult>>("rory.matrixroomutils.homeserver_resolution_cache") ?? new();
        Console.WriteLine($"[LocalStorageWrapper] Loaded {RuntimeCache.LoginSessions.Count} login sessions, {RuntimeCache.HomeserverResolutionCache.Count} homeserver resolution cache entries");
        if (RuntimeCache.AccessToken != null && RuntimeCache.CurrentHomeserver != null)
        {
            Console.WriteLine($"Access token and current homeserver are not null, creating authenticated home server");
            RuntimeCache.CurrentHomeServer = new AuthenticatedHomeServer(RuntimeCache.LoginSessions[RuntimeCache.AccessToken].LoginResponse.UserId, RuntimeCache.AccessToken, RuntimeCache.LoginSessions[RuntimeCache.AccessToken].LoginResponse.HomeServer);
            Console.WriteLine("Created authenticated home server");
        }
        RuntimeCache.WasLoaded = true;
    }

    public static async Task SaveToLocalStorage(ILocalStorageService localStorage)
    {
        await localStorage.SetItemAsStringAsync("rory.matrixroomutils.token", RuntimeCache.AccessToken);
        await localStorage.SetItemAsync("rory.matrixroomutils.current_homeserver", RuntimeCache.CurrentHomeserver);
        await localStorage.SetItemAsync("rory.matrixroomutils.user_cache", RuntimeCache.LoginSessions);
        await localStorage.SetItemAsync("rory.matrixroomutils.homeserver_resolution_cache", 
            RuntimeCache.HomeserverResolutionCache.DistinctBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value));
    }
}