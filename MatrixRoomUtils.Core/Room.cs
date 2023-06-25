using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Web;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.RoomTypes;

namespace MatrixRoomUtils.Core;

public class Room {
    private readonly HttpClient _httpClient;

    public Room(HttpClient httpClient, string roomId) {
        _httpClient = httpClient;
        RoomId = roomId;
        if(GetType() != typeof(SpaceRoom))
            AsSpace = new SpaceRoom(_httpClient, RoomId);
    }

    public string RoomId { get; set; }

    public async Task<JsonElement?> GetStateAsync(string type, string stateKey = "", bool logOnFailure = true) {
        var url = $"/_matrix/client/v3/rooms/{RoomId}/state";
        if (!string.IsNullOrEmpty(type)) url += $"/{type}";
        if (!string.IsNullOrEmpty(stateKey)) url += $"/{stateKey}";

        var res = await _httpClient.GetAsync(url);
        if (!res.IsSuccessStatusCode) {
            if (logOnFailure) Console.WriteLine($"{RoomId}/{stateKey}/{type} - got status: {res.StatusCode}");
            return null;
        }

        var result = await res.Content.ReadFromJsonAsync<JsonElement>();
        return result;
    }

    public async Task<T?> GetStateAsync<T>(string type, string stateKey = "", bool logOnFailure = false) {
        var res = await GetStateAsync(type, stateKey, logOnFailure);
        if (res == null) return default;
        return res.Value.Deserialize<T>();
    }

    public async Task<MessagesResponse> GetMessagesAsync(string from = "", int limit = 10, string dir = "b", string filter = "") {
        var url = $"/_matrix/client/v3/rooms/{RoomId}/messages?from={from}&limit={limit}&dir={dir}";
        if (!string.IsNullOrEmpty(filter)) url += $"&filter={filter}";
        var res = await _httpClient.GetAsync(url);
        if (!res.IsSuccessStatusCode) {
            Console.WriteLine($"Failed to get messages for {RoomId} - got status: {res.StatusCode}");
            throw new Exception($"Failed to get messages for {RoomId} - got status: {res.StatusCode}");
        }

        var result = await res.Content.ReadFromJsonAsync<MessagesResponse>();
        return result ?? new MessagesResponse();
    }

    public async Task<string> GetNameAsync() {
        var res = await GetStateAsync("m.room.name");
        if (!res.HasValue) {
            Console.WriteLine($"Room {RoomId} has no name!");
            return RoomId;
        }

        var resn = res?.TryGetProperty("name", out var name) ?? false ? name.GetString() ?? RoomId : RoomId;
        //Console.WriteLine($"Got name: {resn}");
        return resn;
    }

    public async Task JoinAsync(string[]? homeservers = null, string? reason = null) {
        var join_url = $"/_matrix/client/v3/join/{HttpUtility.UrlEncode(RoomId)}";
        Console.WriteLine($"Calling {join_url} with {homeservers?.Length ?? 0} via's...");
        if (homeservers == null || homeservers.Length == 0) homeservers = new[] { RoomId.Split(':')[1] };
        var fullJoinUrl = $"{join_url}?server_name=" + string.Join("&server_name=", homeservers);
        var res = await _httpClient.PostAsJsonAsync(fullJoinUrl, new {
            reason
        });
    }

    public async Task<List<string>> GetMembersAsync(bool joinedOnly = true) {
        var res = await GetStateAsync("");
        if (!res.HasValue) return new List<string>();
        var members = new List<string>();
        foreach (var member in res.Value.EnumerateArray()) {
            if (member.GetProperty("type").GetString() != "m.room.member") continue;
            if (joinedOnly && member.GetProperty("content").GetProperty("membership").GetString() != "join") continue;
            var memberId = member.GetProperty("state_key").GetString();
            members.Add(memberId ?? throw new InvalidOperationException("Event type was member but state key was null!"));
        }

        return members;
    }

    public async Task<List<string>> GetAliasesAsync() {
        var res = await GetStateAsync("m.room.aliases");
        if (!res.HasValue) return new List<string>();
        var aliases = new List<string>();
        foreach (var alias in res.Value.GetProperty("aliases").EnumerateArray()) aliases.Add(alias.GetString() ?? "");

        return aliases;
    }

    public async Task<string> GetCanonicalAliasAsync() {
        var res = await GetStateAsync("m.room.canonical_alias");
        if (!res.HasValue) return "";
        return res.Value.GetProperty("alias").GetString() ?? "";
    }

    public async Task<string> GetTopicAsync() {
        var res = await GetStateAsync("m.room.topic");
        if (!res.HasValue) return "";
        return res.Value.GetProperty("topic").GetString() ?? "";
    }

    public async Task<string> GetAvatarUrlAsync() {
        var res = await GetStateAsync("m.room.avatar");
        if (!res.HasValue) return "";
        return res.Value.GetProperty("url").GetString() ?? "";
    }

    public async Task<JoinRules> GetJoinRuleAsync() {
        var res = await GetStateAsync("m.room.join_rules");
        if (!res.HasValue) return new JoinRules();
        return res.Value.Deserialize<JoinRules>() ?? new JoinRules();
    }

    public async Task<string> GetHistoryVisibilityAsync() {
        var res = await GetStateAsync("m.room.history_visibility");
        if (!res.HasValue) return "";
        return res.Value.GetProperty("history_visibility").GetString() ?? "";
    }

    public async Task<string> GetGuestAccessAsync() {
        var res = await GetStateAsync("m.room.guest_access");
        if (!res.HasValue) return "";
        return res.Value.GetProperty("guest_access").GetString() ?? "";
    }

    public async Task<CreateEvent> GetCreateEventAsync() {
        var res = await GetStateAsync("m.room.create");
        if (!res.HasValue) return new CreateEvent();

        res.FindExtraJsonElementFields(typeof(CreateEvent));

        return res.Value.Deserialize<CreateEvent>() ?? new CreateEvent();
    }

    public async Task<string?> GetRoomType() {
        var res = await GetStateAsync("m.room.create");
        if (!res.HasValue) return null;
        if (res.Value.TryGetProperty("type", out var type)) return type.GetString();
        return null;
    }
    
    public async Task ForgetAsync() {
        var res = await _httpClient.PostAsync($"/_matrix/client/v3/rooms/{RoomId}/forget", null);
        if (!res.IsSuccessStatusCode) {
            Console.WriteLine($"Failed to forget room {RoomId} - got status: {res.StatusCode}");
            throw new Exception($"Failed to forget room {RoomId} - got status: {res.StatusCode}");
        }
    }
    
    public async Task LeaveAsync(string? reason = null) {
        var res = await _httpClient.PostAsJsonAsync($"/_matrix/client/v3/rooms/{RoomId}/leave", new {
            reason
        });
        if (!res.IsSuccessStatusCode) {
            Console.WriteLine($"Failed to leave room {RoomId} - got status: {res.StatusCode}");
            throw new Exception($"Failed to leave room {RoomId} - got status: {res.StatusCode}");
        }
    }
    
    public async Task KickAsync(string userId, string? reason = null) {
       
        var res = await _httpClient.PostAsJsonAsync($"/_matrix/client/v3/rooms/{RoomId}/kick", new UserIdAndReason() { UserId = userId, Reason = reason });
        if (!res.IsSuccessStatusCode) {
            Console.WriteLine($"Failed to kick {userId} from room {RoomId} - got status: {res.StatusCode}");
            throw new Exception($"Failed to kick {userId} from room {RoomId} - got status: {res.StatusCode}");
        }
    }
    
    public async Task BanAsync(string userId, string? reason = null) {
        var res = await _httpClient.PostAsJsonAsync($"/_matrix/client/v3/rooms/{RoomId}/ban", new UserIdAndReason() { UserId = userId, Reason = reason });
        if (!res.IsSuccessStatusCode) {
            Console.WriteLine($"Failed to ban {userId} from room {RoomId} - got status: {res.StatusCode}");
            throw new Exception($"Failed to ban {userId} from room {RoomId} - got status: {res.StatusCode}");
        }
    }
    
    public async Task UnbanAsync(string userId) {
        var res = await _httpClient.PostAsJsonAsync($"/_matrix/client/v3/rooms/{RoomId}/unban", new UserIdAndReason() { UserId = userId });
        if (!res.IsSuccessStatusCode) {
            Console.WriteLine($"Failed to unban {userId} from room {RoomId} - got status: {res.StatusCode}");
            throw new Exception($"Failed to unban {userId} from room {RoomId} - got status: {res.StatusCode}");
        }
    }
    
    public async Task<EventIdResponse> SendStateEventAsync(string eventType, object content) {
        var res = await _httpClient.PostAsJsonAsync($"/_matrix/client/v3/rooms/{RoomId}/state/{eventType}", content);
        if (!res.IsSuccessStatusCode) {
            Console.WriteLine($"Failed to send state event {eventType} to room {RoomId} - got status: {res.StatusCode}");
            throw new Exception($"Failed to send state event {eventType} to room {RoomId} - got status: {res.StatusCode}");
        }
        return await res.Content.ReadFromJsonAsync<EventIdResponse>();
    }
    
    public async Task<EventIdResponse> SendMessageEventAsync(string eventType, object content) {
        var res = await _httpClient.PutAsJsonAsync($"/_matrix/client/v3/rooms/{RoomId}/send/{eventType}/"+new Guid(), content);
        if (!res.IsSuccessStatusCode) {
            Console.WriteLine($"Failed to send event {eventType} to room {RoomId} - got status: {res.StatusCode}");
            throw new Exception($"Failed to send event {eventType} to room {RoomId} - got status: {res.StatusCode}");
        }
        return await res.Content.ReadFromJsonAsync<EventIdResponse>();
    }


    public readonly SpaceRoom AsSpace;
}