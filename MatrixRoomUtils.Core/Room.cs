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
    private readonly HttpClient _httpClient;
    public string RoomId { get; set; }

    public Room(HttpClient httpClient, string roomId)
    {
        _httpClient = httpClient;
        RoomId = roomId;
    }

    public async Task<JsonElement?> GetStateAsync(string type, string stateKey = "", bool logOnFailure = true)
    {
        var url = $"/_matrix/client/v3/rooms/{RoomId}/state";
        if (!string.IsNullOrEmpty(type)) url += $"/{type}";
        if (!string.IsNullOrEmpty(stateKey)) url += $"/{stateKey}";

        var res = await _httpClient.GetAsync(url);
        if (!res.IsSuccessStatusCode)
        {
            if (logOnFailure) Console.WriteLine($"{RoomId}/{stateKey}/{type} - got status: {res.StatusCode}");
            return null;
        }

        var result = await res.Content.ReadFromJsonAsync<JsonElement>();
        return result;
    }

    public async Task<T?> GetStateAsync<T>(string type, string stateKey = "", bool logOnFailure = false)
    {
        var res = await GetStateAsync(type, stateKey, logOnFailure);
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
        Console.WriteLine($"Calling {join_url} with {homeservers?.Length ?? 0} via's...");
        if (homeservers == null || homeservers.Length == 0) homeservers = new[] { RoomId.Split(':')[1] };
        var fullJoinUrl = $"{join_url}?server_name=" + string.Join("&server_name=", homeservers);
        var res = await _httpClient.PostAsync(fullJoinUrl, null);
    }

    public async Task<List<string>> GetMembersAsync(bool joinedOnly = true)
    {
        var res = await GetStateAsync("");
        if (!res.HasValue) return new List<string>();
        var members = new List<string>();
        foreach (var member in res.Value.EnumerateArray())
        {
            if (member.GetProperty("type").GetString() != "m.room.member") continue;
            if (joinedOnly && member.GetProperty("content").GetProperty("membership").GetString() != "join") continue;
            var memberId = member.GetProperty("state_key").GetString();
            members.Add(memberId ?? throw new InvalidOperationException("Event type was member but state key was null!"));
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
    [JsonPropertyName("creator")] public string Creator { get; set; }
    [JsonPropertyName("room_version")] public string RoomVersion { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("predecessor")] public object? Predecessor { get; set; }
    [JsonPropertyName("m.federate")] public bool Federate { get; set; }
}

public class JoinRules
{
    private const string Public = "public";
    private const string Invite = "invite";
    private const string Knock = "knock";

    [JsonPropertyName("join_rule")] public string JoinRule { get; set; }
    [JsonPropertyName("allow")] public List<string> Allow { get; set; }
}