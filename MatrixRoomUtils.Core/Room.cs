using System.Net.Http.Json;
using System.Text.Json;

namespace MatrixRoomUtils.Core;

public class Room
{
    private readonly HttpClient _httpClient;
    public string RoomId { get; set; }

    public Room(HttpClient httpClient, string roomId)
    {
        _httpClient = httpClient;
        RoomId = roomId;
    }
    
    public async Task<JsonElement?> GetStateAsync(string type, string state_key="", bool logOnFailure = false)
    {
        var url = $"/_matrix/client/v3/rooms/{RoomId}/state";
        if (!string.IsNullOrEmpty(state_key)) url += $"/{type}/{state_key}";
        else if (!string.IsNullOrEmpty(type)) url += $"/{type}";
        var cache_key = "room_states_"+type;
        if (!RuntimeCache.GenericResponseCache.ContainsKey(cache_key))
        {
            Console.WriteLine($"[!!] No cache for {cache_key}, creating...");
            RuntimeCache.GenericResponseCache.Add(cache_key, new ObjectCache<object?>()
            {
                DefaultExpiry = type switch
                {
                    "m.room.name" => TimeSpan.FromMinutes(15),
                    _ => TimeSpan.FromMinutes(5)
                }
            });
        }

        if (RuntimeCache.GenericResponseCache[cache_key][url] != null)
        {
            if(RuntimeCache.GenericResponseCache[cache_key][url].ExpiryTime > DateTime.Now)
            {
                // Console.WriteLine($"[:3] Found cached state: {RuntimeCache.GenericResponseCache[cache_key][url].Result}");
                return (JsonElement?)RuntimeCache.GenericResponseCache[cache_key][url].Result;
            }
            else
            {
                Console.WriteLine($"[!!] Cached state expired at {RuntimeCache.GenericResponseCache[cache_key][url].ExpiryTime}: {RuntimeCache.GenericResponseCache[cache_key][url].Result}");
            }
        }
        else
        {
            Console.WriteLine($"[!!] No cached state for {url}");
        }

        var res = await _httpClient.GetAsync(url);
        if (!res.IsSuccessStatusCode)
        {
            if(logOnFailure) Console.WriteLine($"{RoomId}/{state_key}/{type} - got status: {res.StatusCode}");
            return null;
        }
        var result = await res.Content.ReadFromJsonAsync<JsonElement>();
        RuntimeCache.GenericResponseCache[cache_key][url] = new GenericResult<object>()
        {
            Result = result
        };
        return result;
    }
    public async Task<string?> GetNameAsync()
    {
        var res = await GetStateAsync("m.room.name");
        if (!res.HasValue)
        {
            Console.WriteLine($"Room {RoomId} has no name!");
            return null;
        }
        var resn = res?.TryGetProperty("name", out var name) ?? false ? name.GetString() : null;
        //Console.WriteLine($"Got name: {resn}");
        return resn;
    }
    
}