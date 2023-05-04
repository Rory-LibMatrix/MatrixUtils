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
        var url = $"/_matrix/client/r0/rooms/{RoomId}/state";
        if (!string.IsNullOrEmpty(state_key)) url += $"/{type}/{state_key}";
        else if (!string.IsNullOrEmpty(type)) url += $"/{type}";
        
        var res = await _httpClient.GetAsync(url);
        if (!res.IsSuccessStatusCode)
        {
            if(logOnFailure) Console.WriteLine($"{RoomId}/{state_key}/{type} - got status: {res.StatusCode}");
            return null;
        }
        return await res.Content.ReadFromJsonAsync<JsonElement>();
    }
    public async Task<string?> GetNameAsync()
    {
        var res = await GetStateAsync("m.room.name");
        if (!res.HasValue)
        {
            return null;
        }
        var resn = res?.TryGetProperty("name", out var name) ?? false ? name.GetString() : null;
        Console.WriteLine($"Got name: {resn}");
        return resn;
    }
    
}