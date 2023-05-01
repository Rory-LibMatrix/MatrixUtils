using Blazored.LocalStorage;
using MatrixRoomUtils.Authentication;
using MatrixRoomUtils.Responses;

namespace MatrixRoomUtils.Web.Classes;

public class RuntimeStorage
{
    public static bool WasLoaded = false;
    public static UserInfo? CurrentUserInfo { get; set; }
    public static string AccessToken { get; set; }
    public static string? CurrentHomeserver { get; set; }
    
    public static List<string> AccessTokens { get; set; } = new();
    //public static AppSettings AppSettings { get; set; } = new();

    public static Dictionary<string, UserInfo> UsersCache { get; set; } = new();

    public static Dictionary<string, HomeServerResolutionResult> HomeserverResolutionCache { get; set; } = new();


    //some basic logic
    public static async Task LoadFromLocalStorage(ILocalStorageService localStorage)
    {
        AccessToken = await localStorage.GetItemAsync<string>("rory.matrixroomutils.token");
        CurrentHomeserver = await localStorage.GetItemAsync<string>("rory.matrixroomutils.current_homeserver");
        AccessTokens = await localStorage.GetItemAsync<List<string>>("rory.matrixroomutils.tokens") ?? new();
        UsersCache = await localStorage.GetItemAsync<Dictionary<string, UserInfo>>("rory.matrixroomutils.user_cache") ?? new();
        HomeserverResolutionCache = await localStorage.GetItemAsync<Dictionary<string, HomeServerResolutionResult>>("rory.matrixroomutils.homeserver_resolution_cache") ?? new();
        WasLoaded = true;
    }

    public static async Task SaveToLocalStorage(ILocalStorageService localStorage)
    {
        await localStorage.SetItemAsStringAsync("rory.matrixroomutils.token", AccessToken);
        await localStorage.SetItemAsync("rory.matrixroomutils.current_homeserver", CurrentHomeserver);
        await localStorage.SetItemAsync("rory.matrixroomutils.tokens", AccessTokens);
        await localStorage.SetItemAsync("rory.matrixroomutils.user_cache", UsersCache);
        await localStorage.SetItemAsync("rory.matrixroomutils.homeserver_resolution_cache", 
            HomeserverResolutionCache.DistinctBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value));
    }
}

public class UserInfo
{
    public ProfileResponse Profile { get; set; } = new();
    public LoginResponse LoginResponse { get; set; }
}

public class HomeServerResolutionResult
{
    public string Result { get; set; }
    public DateTime ResolutionTime { get; set; }
}