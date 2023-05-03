using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;
using MatrixRoomUtils.Core.Responses;

namespace MatrixRoomUtils.Core;

public class RemoteHomeServer : IHomeServer
{
    public RemoteHomeServer(string canonicalHomeServerDomain)
    {
        HomeServerDomain = canonicalHomeServerDomain;
        _httpClient = new HttpClient();
    }
    public async Task<RemoteHomeServer> Configure()
    {
        FullHomeServerDomain = await ResolveHomeserverFromWellKnown(HomeServerDomain);
        _httpClient.Dispose();
        _httpClient = new HttpClient { BaseAddress = new Uri(FullHomeServerDomain) };
        Console.WriteLine("[RHS] Finished setting up http client");

        return this;
    }
    public async Task<ProfileResponse> GetProfile(string mxid)
    {
        var resp = await _httpClient.GetAsync($"/_matrix/client/r0/profile/{mxid}");
        var data = await resp.Content.ReadFromJsonAsync<JsonElement>();
        if(!resp.IsSuccessStatusCode) Console.WriteLine("Profile: " + data.ToString());
        return data.Deserialize<ProfileResponse>();
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