using Blazored.LocalStorage;
using MatrixRoomUtils.Core;

namespace MatrixRoomUtils.Web.Classes;

public class LocalStorageWrapper {
    private static readonly SemaphoreSlim _semaphoreSlim = new(1);
    public static Settings Settings { get; set; } = new();

    //some basic logic
    public static async Task InitialiseRuntimeVariables(ILocalStorageService localStorage) {
        //RuntimeCache stuff
        // async Task Save() => await SaveToLocalStorage(localStorage);
        // async Task SaveObject(string key, object obj) => await localStorage.SetItemAsync(key, obj);
        // async Task RemoveObject(string key) => await localStorage.RemoveItemAsync(key);
        //
        // RuntimeCache.Save = Save;
        // RuntimeCache.SaveObject = SaveObject;
        // RuntimeCache.RemoveObject = RemoveObject;
        if (RuntimeCache.LastUsedToken != null) {
            Console.WriteLine("Access token is not null, creating authenticated home server");
            Console.WriteLine($"Homeserver cache: {RuntimeCache.HomeserverResolutionCache.Count} entries");
            // Console.WriteLine(RuntimeCache.HomeserverResolutionCache.ToJson());
            RuntimeCache.CurrentHomeServer = await new AuthenticatedHomeServer(RuntimeCache.LoginSessions[RuntimeCache.LastUsedToken].LoginResponse.HomeServer, RuntimeCache.LastUsedToken, TODO).Configure();
            Console.WriteLine("Created authenticated home server");
        }
    }

    public static async Task LoadFromLocalStorage(ILocalStorageService localStorage) {
        await _semaphoreSlim.WaitAsync();
        if (RuntimeCache.WasLoaded) {
            _semaphoreSlim.Release();
            return;
        }

        Console.WriteLine("Loading from local storage...");
        Settings = await localStorage.GetItemAsync<Settings>("rory.matrixroomutils.settings") ?? new Settings();

        RuntimeCache.LastUsedToken = await localStorage.GetItemAsync<string>("rory.matrixroomutils.last_used_token");
        RuntimeCache.LoginSessions = await localStorage.GetItemAsync<Dictionary<string, UserInfo>>("rory.matrixroomutils.login_sessions") ?? new Dictionary<string, UserInfo>();
        RuntimeCache.HomeserverResolutionCache = await localStorage.GetItemAsync<Dictionary<string, HomeServerResolutionResult>>("rory.matrixroomutils.homeserver_resolution_cache") ?? new Dictionary<string, HomeServerResolutionResult>();
        Console.WriteLine($"[LocalStorageWrapper] Loaded {RuntimeCache.LoginSessions.Count} login sessions, {RuntimeCache.HomeserverResolutionCache.Count} homeserver resolution cache entries");

        //RuntimeCache.GenericResponseCache = await localStorage.GetItemAsync<Dictionary<string, ObjectCache<object>>>("rory.matrixroomutils.generic_cache") ?? new();

        foreach (var s in (await localStorage.KeysAsync()).Where(x => x.StartsWith("rory.matrixroomutils.generic_cache:")).ToList()) {
            Console.WriteLine($"Loading generic cache entry {s}");
            RuntimeCache.GenericResponseCache[s.Replace("rory.matrixroomutils.generic_cache:", "")] = await localStorage.GetItemAsync<ObjectCache<object>>(s);
        }

        await InitialiseRuntimeVariables(localStorage);
        RuntimeCache.WasLoaded = true;
        _semaphoreSlim.Release();
    }

    public static async Task SaveToLocalStorage(ILocalStorageService localStorage) {
        Console.WriteLine("Saving to local storage...");
        await localStorage.SetItemAsync("rory.matrixroomutils.settings", Settings);
        if (RuntimeCache.LoginSessions != null) await localStorage.SetItemAsync("rory.matrixroomutils.login_sessions", RuntimeCache.LoginSessions);
        if (RuntimeCache.LastUsedToken != null) await localStorage.SetItemAsync("rory.matrixroomutils.last_used_token", RuntimeCache.LastUsedToken);
    }

    public static async Task SaveCacheToLocalStorage(ILocalStorageService localStorage, bool awaitSave = true, bool saveGenericCache = true) {
        await localStorage.SetItemAsync("rory.matrixroomutils.homeserver_resolution_cache",
            RuntimeCache.HomeserverResolutionCache.DistinctBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value));
        //await localStorage.SetItemAsync("rory.matrixroomutils.generic_cache", RuntimeCache.GenericResponseCache);
        if (saveGenericCache)
            foreach (var s in RuntimeCache.GenericResponseCache.Keys) {
                var t = localStorage.SetItemAsync($"rory.matrixroomutils.generic_cache:{s}", RuntimeCache.GenericResponseCache[s]);
                if (awaitSave) await t;
            }
    }
}

public class Settings {
    public DeveloperSettings DeveloperSettings { get; set; } = new();
}

public class DeveloperSettings {
    public bool EnableLogViewers { get; set; } = false;
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnablePortableDevtools { get; set; } = false;
}