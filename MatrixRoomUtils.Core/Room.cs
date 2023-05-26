using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using MatrixRoomUtils.Core.Extensions;

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
        var stateCombo = "";
        if (!string.IsNullOrEmpty(state_key)) stateCombo += $"{type}/{state_key}";
        else if (!string.IsNullOrEmpty(type)) stateCombo += $"{type}";
        if (!string.IsNullOrEmpty(stateCombo)) url += $"/{stateCombo}";
        var cache_key = "room_states#" + RoomId;
        if (!RuntimeCache.GenericResponseCache.ContainsKey(cache_key))
        {
            Console.WriteLine($"[!!] No cache for {cache_key}, creating...");
            RuntimeCache.GenericResponseCache.Add(cache_key, new ObjectCache<object?>()
            {
                Name = cache_key
            });
        }
        var cache = RuntimeCache.GenericResponseCache[cache_key];

        if (cache.ContainsKey(stateCombo))
        {
            if (cache[stateCombo].ExpiryTime > DateTime.Now)
            {
                // Console.WriteLine($"[:3] Found cached state: {RuntimeCache.GenericResponseCache[cache_key][url].Result}");
                _semaphore.Release();
                return (JsonElement?) cache[stateCombo].Result;
            }
            else
            {
                Console.WriteLine($"[!!] Cached state expired at {cache[stateCombo].ExpiryTime}: {cache[stateCombo].Result}");
                if(cache[stateCombo].ExpiryTime == null)Console.WriteLine("Exiryt time was null");
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
        var expiryTime = type switch
        {
            "m.room.name" => TimeSpan.FromMinutes(30),
            "org.matrix.mjolnir.shortcode" => TimeSpan.FromHours(4),
            "" => TimeSpan.FromSeconds(0),
            _ => TimeSpan.FromMinutes(15)
        };
        if(!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(state_key))
            cache[stateCombo] = new GenericResult<object>()
            {
                Result = result,
                ExpiryTime = DateTime.Now.Add(expiryTime)
            };
        _semaphore.Release();
        return result;
    }
    public async Task<T?> GetStateAsync<T>(string type, string state_key = "", bool logOnFailure = false)
    {
        var res = await GetStateAsync(type, state_key, logOnFailure);
        if (res == null) return default;
        return res.Value.Deserialize<T>();
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
    
    public async Task<List<string>> GetMembersAsync()
    {
        var res = await GetStateAsync("");
        if (!res.HasValue) return new List<string>();
        var members = new List<string>();
        foreach (var member in res.Value.EnumerateArray())
        {
            if(member.GetProperty("Type").GetString() != "m.room.member") continue;
            var member_id = member.GetProperty("StateKey").GetString();
            members.Add(member_id);
        }

        return members;
    }
    
    public async Task<List<string>> GetAliasesAsync()
    {
        var res = await GetStateAsync("m.room.aliases");
        if (!res.HasValue) return new List<string>();
        var aliases = new List<string>();
        foreach (var alias in res.Value.GetProperty("aliases").EnumerateArray())
        {
            aliases.Add(alias.GetString() ?? "");
        }

        return aliases;
    }
    
    public async Task<string> GetCanonicalAliasAsync()
    {
        var res = await GetStateAsync("m.room.canonical_alias");
        if (!res.HasValue) return "";
        return res.Value.GetProperty("alias").GetString() ?? "";
    }
    
    public async Task<string> GetTopicAsync()
    {
        var res = await GetStateAsync("m.room.topic");
        if (!res.HasValue) return "";
        return res.Value.GetProperty("topic").GetString() ?? "";
    }
    
    public async Task<string> GetAvatarUrlAsync()
    {
        var res = await GetStateAsync("m.room.avatar");
        if (!res.HasValue) return "";
        return res.Value.GetProperty("url").GetString() ?? "";
    }
    
    public async Task<JoinRules> GetJoinRuleAsync()
    {
        var res = await GetStateAsync("m.room.join_rules");
        if (!res.HasValue) return new JoinRules();
        return res.Value.Deserialize<JoinRules>() ?? new JoinRules();
    }
    
    public async Task<string> GetHistoryVisibilityAsync()
    {
        var res = await GetStateAsync("m.room.history_visibility");
        if (!res.HasValue) return "";
        return res.Value.GetProperty("history_visibility").GetString() ?? "";
    }
    
    public async Task<string> GetGuestAccessAsync()
    {
        var res = await GetStateAsync("m.room.guest_access");
        if (!res.HasValue) return "";
        return res.Value.GetProperty("guest_access").GetString() ?? "";
    }
    
    public async Task<CreateEvent> GetCreateEventAsync()
    {
        var res = await GetStateAsync("m.room.create");
        if (!res.HasValue) return new CreateEvent();
        
        res.FindExtraJsonFields(typeof(CreateEvent));

        return res.Value.Deserialize<CreateEvent>() ?? new CreateEvent();
    }
}

public class CreateEvent
{
    [JsonPropertyName("creator")]
    public string Creator { get; set; }
    [JsonPropertyName("room_version")]
    public string RoomVersion { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("predecessor")]
    public object? Predecessor { get; set; }
    
    [JsonPropertyName("m.federate")]
    public bool Federate { get; set; }
}

public class JoinRules
{
    private const string Public = "public";
    private const string Invite = "invite";
    private const string Knock = "knock";
    
    [JsonPropertyName("join_rule")]
    public string JoinRule { get; set; }
    [JsonPropertyName("allow")]
    public List<string> Allow { get; set; }
}