using Blazored.LocalStorage;
using MatrixRoomUtils.Core;

namespace MatrixRoomUtils.Web.Classes;

public partial class LocalStorageWrapper
{
    private static SemaphoreSlim _semaphoreSlim = new(1);
    public static Settings Settings { get; set; } = new();
    
    //some basic logic
    public static async Task ReloadLocalStorage(ILocalStorageService localStorage)
    {
        await SaveToLocalStorage(localStorage);
        await LoadFromLocalStorage(localStorage);
    }
    public static async Task LoadFromLocalStorage(ILocalStorageService localStorage)
    {
        await _semaphoreSlim.WaitAsync();
        if (RuntimeCache.WasLoaded) return;
        RuntimeCache.WasLoaded = true;
        Settings = await localStorage.GetItemAsync<Settings>("rory.matrixroomutils.settings") ?? new();
        
        //RuntimeCache stuff
        async void Save() => await SaveToLocalStorage(localStorage);

        RuntimeCache.Save = Save;
        RuntimeCache.SaveObject = async (key, obj) => await localStorage.SetItemAsync(key, obj); 
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
            // Console.WriteLine(RuntimeCache.HomeserverResolutionCache.ToJson());
            RuntimeCache.CurrentHomeServer = await new AuthenticatedHomeServer(RuntimeCache.LoginSessions[RuntimeCache.LastUsedToken].LoginResponse.UserId, RuntimeCache.LastUsedToken, RuntimeCache.LoginSessions[RuntimeCache.LastUsedToken].LoginResponse.HomeServer).Configure();
            Console.WriteLine("Created authenticated home server");
        }
        RuntimeCache.GenericResponseCache = await localStorage.GetItemAsync<Dictionary<string, ObjectCache<object>>>("rory.matrixroomutils.generic_cache") ?? new();
        
        foreach (var s in (await localStorage.KeysAsync()).Where(x=>x.StartsWith("rory.matrixroomutils.generic_cache:")).ToList())
        {
            Console.WriteLine($"Loading generic cache entry {s}");
            RuntimeCache.GenericResponseCache[s.Replace("rory.matrixroomutils.generic_cache:", "")] = await localStorage.GetItemAsync<ObjectCache<object>>(s);
        }

        _semaphoreSlim.Release();
    }

    public static async Task SaveToLocalStorage(ILocalStorageService localStorage)
    {
        await localStorage.SetItemAsync("rory.matrixroomutils.settings", Settings);
        // if(RuntimeCache.AccessToken != null) await localStorage.SetItemAsStringAsync("rory.matrixroomutils.token", RuntimeCache.AccessToken);
        // if(RuntimeCache.CurrentHomeserver != null) await localStorage.SetItemAsync("rory.matrixroomutils.current_homeserver", RuntimeCache.CurrentHomeserver);
        if(RuntimeCache.LoginSessions != null) await localStorage.SetItemAsync("rory.matrixroomutils.user_cache", RuntimeCache.LoginSessions);
        if(RuntimeCache.LastUsedToken != null) await localStorage.SetItemAsync("rory.matrixroomutils.last_used_token", RuntimeCache.LastUsedToken);
        await localStorage.SetItemAsync("rory.matrixroomutils.homeserver_resolution_cache", 
            RuntimeCache.HomeserverResolutionCache.DistinctBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value));
        await localStorage.SetItemAsync("rory.matrixroomutils.generic_cache", RuntimeCache.GenericResponseCache);
        // foreach (var s in RuntimeCache.GenericResponseCache.Keys)
        // {
            // await localStorage.SetItemAsync($"rory.matrixroomutils.generic_cache:{s}", RuntimeCache.GenericResponseCache[s]);
        // }
    }
    public static async Task SaveFieldToLocalStorage(ILocalStorageService localStorage, string key)
    {
        if (key == "rory.matrixroomutils.settings") await localStorage.SetItemAsync(key, Settings);
        // if (key == "rory.matrixroomutils.token") await localStorage.SetItemAsStringAsync(key, RuntimeCache.AccessToken);
        // if (key == "rory.matrixroomutils.current_homeserver") await localStorage.SetItemAsync(key, RuntimeCache.CurrentHomeserver);
        if (key == "rory.matrixroomutils.user_cache") await localStorage.SetItemAsync(key, RuntimeCache.LoginSessions);
        if (key == "rory.matrixroomutils.last_used_token") await localStorage.SetItemAsync(key, RuntimeCache.LastUsedToken);
        if (key == "rory.matrixroomutils.homeserver_resolution_cache") await localStorage.SetItemAsync(key, RuntimeCache.HomeserverResolutionCache);
        //if (key == "rory.matrixroomutils.generic_cache") await localStorage.SetItemAsync(key, RuntimeCache.GenericResponseCache);
    }
}


public class Settings
{
    public DeveloperSettings DeveloperSettings { get; set; } = new();
}


public class DeveloperSettings
{
    public bool EnableLogViewers { get; set; } = false;
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnablePortableDevtools { get; set; } = false;
}