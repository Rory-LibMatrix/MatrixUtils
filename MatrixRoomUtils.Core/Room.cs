using System.Net.Http.Json;
using System.Text.Json;

namespace MatrixRoomUtils;

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
        var res = await _httpClient.GetAsync($"/_matrix/client/r0/rooms/{RoomId}/state/{type}/{state_key}");
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
        var resn = res?.GetProperty("name").GetString();
        Console.WriteLine($"Got name: {resn}");
        return resn;
    }
    
}