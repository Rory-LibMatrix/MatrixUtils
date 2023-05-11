using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Xml.Schema;
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
    public TimeSpan DefaultExpiry { get; set; } = new(0, 5, 0);
    public GenericResult<T> this[string key]
    {
        get
        {
            if (Random.Shared.Next(100) == 1)
            {
                // Console.WriteLine("Cleaning cache...");
                // foreach (var x in Cache.Where(x => x.Value.ExpiryTime < DateTime.Now).OrderBy(x => x.Value.ExpiryTime).Take(3).ToList())
                // {
                    // Console.WriteLine($"Removing {x.Key} from cache");
                    // Cache.Remove(x.Key);   
                // }
            }

            
            if (Cache.ContainsKey(key))
            {
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
            return null;
        }
        set
        {
            Cache[key] = value;
            if(Cache[key].ExpiryTime == null) Cache[key].ExpiryTime = DateTime.Now.Add(DefaultExpiry);
            Console.WriteLine($"New item in cache: {key} - {Cache[key].Result.ToJson(indent: false)}");
            // Console.Error.WriteLine("Full cache: " + Cache.ToJson());
        }
    }
}
public class GenericResult<T>
{
    public T? Result { get; set; }
    public DateTime? ExpiryTime { get; set; }
}
