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
    
    public async Task<JsonElement?> GetStateAsync(string type, string state_key="")
    {
        Console.WriteLine($"{RoomId}::_qry[{type}::{state_key}]");
        var url = $"/_matrix/client/r0/rooms/{RoomId}/state";
        if (!string.IsNullOrEmpty(state_key)) url += $"/{type}/{state_key}";
        else if (!string.IsNullOrEmpty(type)) url += $"/{type}";
        
        var res = await _httpClient.GetAsync(url);
        if (!res.IsSuccessStatusCode)
        {
            Console.WriteLine($"{RoomId}::_qry[{type}::{state_key}]->status=={res.StatusCode}");
            return null;
        }
        return await res.Content.ReadFromJsonAsync<JsonElement>();
    }
    public async Task<string?> GetNameAsync()
    {   
        Console.WriteLine($"{RoomId}::_qry_name");
        var res = await GetStateAsync("m.room.name");
        if (!res.HasValue)
        {
            Console.WriteLine($"{RoomId}::_qry_name->null");
            return null;
        }
        Console.WriteLine($"{RoomId}::_qry_name->{res.Value.ToString()}");
        var resn = res?.TryGetProperty("name", out var name) ?? false ? name.GetString() : null;
        Console.WriteLine($"Got name: {resn}");
        return resn;
    }
    
}