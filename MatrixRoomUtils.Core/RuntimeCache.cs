using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Responses;

namespace MatrixRoomUtils.Core;

public class RuntimeCache
{
    public static bool WasLoaded = false;
    public static string? LastUsedToken { get; set; }
    public static AuthenticatedHomeServer CurrentHomeServer { get; set; }
    public static Dictionary<string, UserInfo> LoginSessions { get; set; } = new();

    public static Dictionary<string, HomeServerResolutionResult> HomeserverResolutionCache { get; set; } = new();
    // public static Dictionary<string, (DateTime cachedAt, ProfileResponse response)> ProfileCache { get; set; } = new();

    public static Dictionary<string, ObjectCache<object>> GenericResponseCache { get; set; } = new();
    
    public static Action Save { get; set; } = () =>
    {
        Console.WriteLine("RuntimeCache.Save() was called, but no callback was set!");
    };
    public static Action<string, object> SaveObject { get; set; } = (key, value) =>
    {
        Console.WriteLine($"RuntimeCache.SaveObject({key}, {value}) was called, but no callback was set!");
    };
}


public class UserInfo
{
    public ProfileResponse Profile { get; set; } = new();
    public LoginResponse LoginResponse { get; set; }
    public string AccessToken { get => LoginResponse.AccessToken; }
}

public class HomeServerResolutionResult
{
    public string Result { get; set; }
    public DateTime ResolutionTime { get; set; }
}
public class ObjectCache<T> where T : class
{
    public Dictionary<string, GenericResult<T>> Cache { get; set; } = new();
    public TimeSpan DefaultExpiry { get; set; } = new(0, 0, 0);
    public string Name { get; set; } = null!;
    public GenericResult<T> this[string key]
    {
        get
        {
            if (Cache.ContainsKey(key))
            {
                Console.WriteLine($"cache.get({key}): hit");
                // Console.WriteLine($"Found item in cache: {key} - {Cache[key].Result.ToJson(indent: false)}");
                if(Cache[key].ExpiryTime > DateTime.Now)
                    return Cache[key];
                Console.WriteLine($"Expired item in cache: {key} - {Cache[key].Result.ToJson(indent: false)}");
                try
                {
                    Cache.Remove(key);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to remove {key} from cache: {e.Message}");
                }
            }
            Console.WriteLine($"cache.get({key}): miss");
            return null;
        }
        set
        {
            Cache[key] = value;
            if(Cache[key].ExpiryTime == null) Cache[key].ExpiryTime = DateTime.Now.Add(DefaultExpiry);
            Console.WriteLine($"set({key}) = {Cache[key].Result.ToJson(indent:false)}");
            Console.WriteLine($"new_state: {this.ToJson(indent:false)}");
            // Console.WriteLine($"New item in cache: {key} - {Cache[key].Result.ToJson(indent: false)}");
            // Console.Error.WriteLine("Full cache: " + Cache.ToJson());
        }
    }
    
    public ObjectCache()
    {
        //expiry timer
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                foreach (var x in Cache.Where(x => x.Value.ExpiryTime < DateTime.Now).OrderBy(x => x.Value.ExpiryTime).Take(15).ToList())
                {
                    // Console.WriteLine($"Removing {x.Key} from cache");
                    Cache.Remove(x.Key);   
                }
                RuntimeCache.SaveObject("rory.matrixroomutils.generic_cache:" + Name, this);
            }
        });
    }

    public bool ContainsKey(string key) => Cache.ContainsKey(key);
}
public class GenericResult<T>
{
    public T? Result { get; set; }
    public DateTime? ExpiryTime { get; set; } = DateTime.Now;
    
    public GenericResult()
    {
        //expiry timer
        
    }
    public GenericResult(T? result, DateTime? expiryTime = null) : this()
    {
        Result = result;
        ExpiryTime = expiryTime;
    }
}
