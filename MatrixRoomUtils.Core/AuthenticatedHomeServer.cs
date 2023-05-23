using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core;

public class AuthenticatedHomeServer : IHomeServer
{
    public string UserId { get; set; }
    public string AccessToken { get; set; }
    public readonly HomeserverAdminApi Admin;

    public AuthenticatedHomeServer(string userId, string accessToken, string canonicalHomeServerDomain)
    {
        UserId = userId;
        AccessToken = accessToken;
        HomeServerDomain = canonicalHomeServerDomain;
        Admin = new HomeserverAdminApi(this);
        _httpClient = new HttpClient();
    }

    public async Task<AuthenticatedHomeServer> Configure()
    {
        FullHomeServerDomain = await ResolveHomeserverFromWellKnown(HomeServerDomain);
        _httpClient.Dispose();
        _httpClient = new HttpClient { BaseAddress = new Uri(FullHomeServerDomain) };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        Console.WriteLine("[AHS] Finished setting up http client");

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
        
        Console.WriteLine($"Fetched {rooms.Count} rooms");

        return rooms;
    }
    
    
    
    
    
    public class HomeserverAdminApi
    {
        private readonly AuthenticatedHomeServer _authenticatedHomeServer;

        public HomeserverAdminApi(AuthenticatedHomeServer authenticatedHomeServer)
        {
            _authenticatedHomeServer = authenticatedHomeServer;
        }
    
    
        
        
        
        
        
    }
}
