using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Filters;
using MatrixRoomUtils.Core.Helpers;
using MatrixRoomUtils.Core.Interfaces;
using MatrixRoomUtils.Core.Responses;
using MatrixRoomUtils.Core.Responses.Admin;
using MatrixRoomUtils.Core.Services;

namespace MatrixRoomUtils.Core;

public class AuthenticatedHomeServer : IHomeServer {
    private readonly TieredStorageService _storage;
    public readonly HomeserverAdminApi Admin;
    public readonly SyncHelper SyncHelper;

    public AuthenticatedHomeServer(string canonicalHomeServerDomain, string accessToken, TieredStorageService storage) {
        _storage = storage;
        AccessToken = accessToken;
        HomeServerDomain = canonicalHomeServerDomain;
        Admin = new HomeserverAdminApi(this);
        SyncHelper = new SyncHelper(this, storage);
        _httpClient = new MatrixHttpClient();
    }

    public string UserId { get; }
    public string AccessToken { get; set; }

    public async Task<AuthenticatedHomeServer> Configure() {
        FullHomeServerDomain = await ResolveHomeserverFromWellKnown(HomeServerDomain);
        _httpClient.Dispose();
        _httpClient = new MatrixHttpClient { BaseAddress = new Uri(FullHomeServerDomain) };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        Console.WriteLine("[AHS] Finished setting up http client");

        return this;
    }

    public async Task<Room> GetRoom(string roomId) => new Room(_httpClient, roomId);

    public async Task<List<Room>> GetJoinedRooms() {
        var rooms = new List<Room>();
        var roomQuery = await _httpClient.GetAsync("/_matrix/client/v3/joined_rooms");

        var roomsJson = await roomQuery.Content.ReadFromJsonAsync<JsonElement>();
        foreach (var room in roomsJson.GetProperty("joined_rooms").EnumerateArray()) rooms.Add(new Room(_httpClient, room.GetString()));

        Console.WriteLine($"Fetched {rooms.Count} rooms");

        return rooms;
    }

    public async Task<string> UploadFile(string fileName, Stream fileStream, string contentType = "application/octet-stream") {
        var res = await _httpClient.PostAsync($"/_matrix/media/r0/upload?filename={fileName}", new StreamContent(fileStream));
        if (!res.IsSuccessStatusCode) {
            Console.WriteLine($"Failed to upload file: {await res.Content.ReadAsStringAsync()}");
            throw new InvalidDataException($"Failed to upload file: {await res.Content.ReadAsStringAsync()}");
        }

        var resJson = await res.Content.ReadFromJsonAsync<JsonElement>();
        return resJson.GetProperty("content_uri").GetString()!;
    }

    public async Task<Room> CreateRoom(CreateRoomRequest creationEvent) {
        var res = await _httpClient.PostAsJsonAsync("/_matrix/client/r0/createRoom", creationEvent);
        if (!res.IsSuccessStatusCode) {
            Console.WriteLine($"Failed to create room: {await res.Content.ReadAsStringAsync()}");
            throw new InvalidDataException($"Failed to create room: {await res.Content.ReadAsStringAsync()}");
        }

        return await GetRoom((await res.Content.ReadFromJsonAsync<JsonObject>())!["room_id"]!.ToString());
    }

    public class HomeserverAdminApi {
        private readonly AuthenticatedHomeServer _authenticatedHomeServer;

        public HomeserverAdminApi(AuthenticatedHomeServer authenticatedHomeServer) => _authenticatedHomeServer = authenticatedHomeServer;

        public async IAsyncEnumerable<AdminRoomListingResult.AdminRoomListingResultRoom> SearchRoomsAsync(int limit = int.MaxValue, string orderBy = "name", string dir = "f", string? searchTerm = null, LocalRoomQueryFilter? localFilter = null) {
            AdminRoomListingResult? res = null;
            var i = 0;
            int? totalRooms = null;
            do {
                var url = $"/_synapse/admin/v1/rooms?limit={Math.Min(limit, 100)}&dir={dir}&order_by={orderBy}";
                if (!string.IsNullOrEmpty(searchTerm)) url += $"&search_term={searchTerm}";

                if (res?.NextBatch != null) url += $"&from={res.NextBatch}";

                Console.WriteLine($"--- ADMIN Querying Room List with URL: {url} - Already have {i} items... ---");

                res = await _authenticatedHomeServer._httpClient.GetFromJsonAsync<AdminRoomListingResult>(url);
                totalRooms ??= res?.TotalRooms;
                Console.WriteLine(res.ToJson(false));
                foreach (var room in res.Rooms) {
                    if (localFilter != null) {
                        if (!room.RoomId.Contains(localFilter.RoomIdContains)) {
                            totalRooms--;
                            continue;
                        }
                        if (!room.Name?.Contains(localFilter.NameContains) == true) {
                            totalRooms--;
                            continue;
                        }
                        if (!room.CanonicalAlias?.Contains(localFilter.CanonicalAliasContains) == true) {
                            totalRooms--;
                            continue;
                        }
                        if (!room.Version.Contains(localFilter.VersionContains)) {
                            totalRooms--;
                            continue;
                        }
                        if (!room.Creator.Contains(localFilter.CreatorContains)) {
                            totalRooms--;
                            continue;
                        }
                        if (!room.Encryption?.Contains(localFilter.EncryptionContains) == true) {
                            totalRooms--;
                            continue;
                        }
                        if (!room.JoinRules?.Contains(localFilter.JoinRulesContains) == true) {
                            totalRooms--;
                            continue;
                        }
                        if(!room.GuestAccess?.Contains(localFilter.GuestAccessContains) == true) {
                            totalRooms--;
                            continue;
                        }
                        if(!room.HistoryVisibility?.Contains(localFilter.HistoryVisibilityContains) == true) {
                            totalRooms--;
                            continue;
                        }
                        
                        if(localFilter.CheckFederation && room.Federatable != localFilter.Federatable) {
                            totalRooms--;
                            continue;
                        }
                        if(localFilter.CheckPublic && room.Public != localFilter.Public) {
                            totalRooms--;
                            continue;
                        }
                        
                        if(room.JoinedMembers < localFilter.JoinedMembersGreaterThan || room.JoinedMembers > localFilter.JoinedMembersLessThan) {
                            totalRooms--;
                            continue;
                        }
                        if(room.JoinedLocalMembers < localFilter.JoinedLocalMembersGreaterThan || room.JoinedLocalMembers > localFilter.JoinedLocalMembersLessThan) {
                            totalRooms--;
                            continue;
                        }
                    }
                    // if (contentSearch != null && !string.IsNullOrEmpty(contentSearch) &&
                    //     !(
                    //         room.Name?.Contains(contentSearch, StringComparison.InvariantCultureIgnoreCase) == true ||
                    //         room.CanonicalAlias?.Contains(contentSearch, StringComparison.InvariantCultureIgnoreCase) == true ||
                    //         room.Creator?.Contains(contentSearch, StringComparison.InvariantCultureIgnoreCase) == true
                    //     )
                    //    ) {
                    //     totalRooms--;
                    //     continue;
                    // }

                    i++;
                    yield return room;
                }
            } while (i < Math.Min(limit, totalRooms ?? limit));
        }
    }
}