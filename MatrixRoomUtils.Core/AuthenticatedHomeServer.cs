using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MatrixRoomUtils;

public class AuthenticatedHomeServer : IHomeServer
{
    public string UserId { get; set; }
    public string AccessToken { get; set; }

    public AuthenticatedHomeServer(string userId, string accessToken, string canonicalHomeServerDomain)
    {
        UserId = userId;
        AccessToken = accessToken;
        HomeServerDomain = canonicalHomeServerDomain;
        _httpClient = new HttpClient();
        
        var rhsfwt = ResolveHomeserverFromWellKnown(canonicalHomeServerDomain);
        rhsfwt.ContinueWith(_ =>
        {
            FullHomeServerDomain = rhsfwt.Result;
            _httpClient.Dispose();
            _httpClient = new HttpClient {BaseAddress = new Uri(FullHomeServerDomain)};
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            Console.WriteLine("[AHS] Finished setting up http client :)");
        });
    }

    public async Task<Room> GetRoom(string roomId)
    {
        return new Room(_httpClient, roomId);
    }

    public async Task<List<Room>> GetJoinedRooms()
    {
        var rooms = new List<Room>();
        var _rooms = await _httpClient.GetAsync("/_matrix/client/v3/joined_rooms");
        if (!_rooms.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to get rooms: {await _rooms.Content.ReadAsStringAsync()}");
            throw new InvalidDataException($"Failed to get rooms: {await _rooms.Content.ReadAsStringAsync()}");
        }

        var roomsJson = await _rooms.Content.ReadFromJsonAsync<JsonElement>();
        foreach (var room in roomsJson.GetProperty("joined_rooms").EnumerateArray())
        {
            rooms.Add(new Room(_httpClient, room.GetString()));
        }

        return rooms;
    }
}