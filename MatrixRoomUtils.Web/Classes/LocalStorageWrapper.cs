using Blazored.LocalStorage;
using MatrixRoomUtils.Core;
using MatrixRoomUtils.Core.Extensions;

namespace MatrixRoomUtils.Web.Classes;

public partial class LocalStorageWrapper
{
    //some basic logic
    public static async Task ReloadLocalStorage(ILocalStorageService localStorage)
    {
        await SaveToLocalStorage(localStorage);
        await LoadFromLocalStorage(localStorage);
    }
    public static async Task LoadFromLocalStorage(ILocalStorageService localStorage)
    {
        // RuntimeCache.AccessToken = await localStorage.GetItemAsync<string>("rory.matrixroomutils.token");
        RuntimeCache.LastUsedToken = await localStorage.GetItemAsync<string>("rory.matrixroomutils.last_used_token");
        // RuntimeCache.CurrentHomeserver = await localStorage.GetItemAsync<string>("rory.matrixroomutils.current_homeserver");
        RuntimeCache.LoginSessions = await localStorage.GetItemAsync<Dictionary<string, UserInfo>>("rory.matrixroomutils.user_cache") ?? new();
        RuntimeCache.HomeserverResolutionCache = await localStorage.GetItemAsync<Dictionary<string, HomeServerResolutionResult>>("rory.matrixroomutils.homeserver_resolution_cache") ?? new();
        Console.WriteLine($"[LocalStorageWrapper] Loaded {RuntimeCache.LoginSessions.Count} login sessions, {RuntimeCache.HomeserverResolutionCache.Count} homeserver resolution cache entries");
        if (RuntimeCache.LastUsedToken != null)
        {
            Console.WriteLine($"Access token is not null, creating authenticated home server");
            Console.WriteLine($"Homeserver cache: {RuntimeCache.HomeserverResolutionCache.Count} entries");
            Console.WriteLine(RuntimeCache.HomeserverResolutionCache.ToJson());
            RuntimeCache.CurrentHomeServer = await new AuthenticatedHomeServer(RuntimeCache.LoginSessions[RuntimeCache.LastUsedToken].LoginResponse.UserId, RuntimeCache.LastUsedToken, RuntimeCache.LoginSessions[RuntimeCache.LastUsedToken].LoginResponse.HomeServer).Configure();
            Console.WriteLine("Created authenticated home server");
        }
        RuntimeCache.WasLoaded = true;
    }

    public static async Task SaveToLocalStorage(ILocalStorageService localStorage)
    {
        // if(RuntimeCache.AccessToken != null) await localStorage.SetItemAsStringAsync("rory.matrixroomutils.token", RuntimeCache.AccessToken);
        // if(RuntimeCache.CurrentHomeserver != null) await localStorage.SetItemAsync("rory.matrixroomutils.current_homeserver", RuntimeCache.CurrentHomeserver);
        if(RuntimeCache.LoginSessions != null) await localStorage.SetItemAsync("rory.matrixroomutils.user_cache", RuntimeCache.LoginSessions);
        if(RuntimeCache.LastUsedToken != null) await localStorage.SetItemAsync("rory.matrixroomutils.last_used_token", RuntimeCache.LastUsedToken);
        await localStorage.SetItemAsync("rory.matrixroomutils.homeserver_resolution_cache", 
            RuntimeCache.HomeserverResolutionCache.DistinctBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value));
    }
}