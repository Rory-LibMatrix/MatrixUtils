using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core;

public class RemoteHomeServer : IHomeServer
{


    public RemoteHomeServer(string canonicalHomeServerDomain)
    {
        HomeServerDomain = canonicalHomeServerDomain;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
    }
    public async Task<RemoteHomeServer> Configure()
    {
        FullHomeServerDomain = await ResolveHomeserverFromWellKnown(HomeServerDomain);
        _httpClient.Dispose();
        _httpClient = new HttpClient { BaseAddress = new Uri(FullHomeServerDomain) };
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
        Console.WriteLine("[RHS] Finished setting up http client");

        return this;
    }
    
    public async Task<Room> GetRoom(string roomId)
    {
        return new Room(_httpClient, roomId);
    }

    public async Task<List<Room>> GetJoinedRooms()
    {
        var rooms = new List<Room>();
        var roomQuery = await _httpClient.GetAsync("/_matrix/client/v3/joined_rooms");
        if (!roomQuery.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to get rooms: {await roomQuery.Content.ReadAsStringAsync()}");
            throw new InvalidDataException($"Failed to get rooms: {await roomQuery.Content.ReadAsStringAsync()}");
        }

        var roomsJson = await roomQuery.Content.ReadFromJsonAsync<JsonElement>();
        foreach (var room in roomsJson.GetProperty("joined_rooms").EnumerateArray())
        {
            rooms.Add(new Room(_httpClient, room.GetString()));
        }

        return rooms;
    }
}