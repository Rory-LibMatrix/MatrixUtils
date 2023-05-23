using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace MatrixRoomUtils.Core;

public class Room
{
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(16, 16);

    private readonly HttpClient _httpClient;
    public string RoomId { get; set; }

    public Room(HttpClient httpClient, string roomId)
    {
        _httpClient = httpClient;
        RoomId = roomId;
    }

    public async Task<JsonElement?> GetStateAsync(string type, string state_key = "", bool logOnFailure = false)
    {
        await _semaphore.WaitAsync();
        var url = $"/_matrix/client/v3/rooms/{RoomId}/state";
        if (!string.IsNullOrEmpty(state_key)) url += $"/{type}/{state_key}";
        else if (!string.IsNullOrEmpty(type)) url += $"/{type}";
        var cache_key = "room_states:" + type;
        if (!RuntimeCache.GenericResponseCache.ContainsKey(cache_key))
        {
            Console.WriteLine($"[!!] No cache for {cache_key}, creating...");
            RuntimeCache.GenericResponseCache.Add(cache_key, new ObjectCache<object?>());
        }

        RuntimeCache.GenericResponseCache[cache_key].DefaultExpiry = type switch
        {
            "m.room.name" => TimeSpan.FromMinutes(30),
            "org.matrix.mjolnir.shortcode" => TimeSpan.FromHours(4),
            "" => TimeSpan.FromSeconds(0),
            _ => TimeSpan.FromMinutes(15)
        };

        if (RuntimeCache.GenericResponseCache[cache_key].Cache.ContainsKey(url) && RuntimeCache.GenericResponseCache[cache_key][url] != null)
        {
            if (RuntimeCache.GenericResponseCache[cache_key][url].ExpiryTime > DateTime.Now)
            {
                // Console.WriteLine($"[:3] Found cached state: {RuntimeCache.GenericResponseCache[cache_key][url].Result}");
                _semaphore.Release();
                return (JsonElement?)RuntimeCache.GenericResponseCache[cache_key][url].Result;
            }
            else
            {
                Console.WriteLine($"[!!] Cached state expired at {RuntimeCache.GenericResponseCache[cache_key][url].ExpiryTime}: {RuntimeCache.GenericResponseCache[cache_key][url].Result}");
            }
        }
        // else
        // {
        //     Console.WriteLine($"[!!] No cached state for {url}");
        // }

        var res = await _httpClient.GetAsync(url);
        if (!res.IsSuccessStatusCode)
        {
            if (logOnFailure) Console.WriteLine($"{RoomId}/{state_key}/{type} - got status: {res.StatusCode}");
            _semaphore.Release();
            return null;
        }

        var result = await res.Content.ReadFromJsonAsync<JsonElement>();

        if (!RuntimeCache.GenericResponseCache.ContainsKey(cache_key) && type != "")
        {
            Console.WriteLine($"[!!] No cache for {cache_key}, creating...");
            RuntimeCache.GenericResponseCache.Add(cache_key, new ObjectCache<object?>());
        }

        RuntimeCache.GenericResponseCache[cache_key][url] = new GenericResult<object>()
        {
            Result = result
        };
        _semaphore.Release();
        return result;
    }

    public async Task<string> GetNameAsync()
    {
        var res = await GetStateAsync("m.room.name");
        if (!res.HasValue)
        {
            Console.WriteLine($"Room {RoomId} has no name!");
            return RoomId;
        }

        var resn = res?.TryGetProperty("name", out var name) ?? false ? name.GetString() ?? RoomId : RoomId;
        //Console.WriteLine($"Got name: {resn}");
        return resn;
    }

    public async Task JoinAsync(string[]? homeservers = null)
    {
        string join_url = $"/_matrix/client/r0/join/{HttpUtility.UrlEncode(RoomId)}";
        Console.WriteLine($"Calling {join_url} with {(homeservers == null ? 0 : homeservers.Length)} via's...");
        if(homeservers == null || homeservers.Length == 0) homeservers = new[] { RoomId.Split(':')[1] };
        var full_join_url = $"{join_url}?server_name=" + string.Join("&server_name=", homeservers);
        var res = await _httpClient.PostAsync(full_join_url, null);
    }
}