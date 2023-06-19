using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Responses;
using MatrixRoomUtils.Core.Services;
using MatrixRoomUtils.Core.StateEventTypes;

namespace MatrixRoomUtils.Core.Helpers;

public class SyncHelper {
    private readonly AuthenticatedHomeServer _homeServer;
    private readonly TieredStorageService _storageService;

    public SyncHelper(AuthenticatedHomeServer homeServer, TieredStorageService storageService) {
        _homeServer = homeServer;
        _storageService = storageService;
    }

    public async Task<SyncResult?> Sync(string? since = null) {
        var outFileName = "sync-" +
                          (await _storageService.CacheStorageProvider.GetAllKeys()).Count(x => x.StartsWith("sync")) +
                          ".json";
        var url = "/_matrix/client/v3/sync?timeout=30000&set_presence=online";
        if (!string.IsNullOrWhiteSpace(since)) url += $"&since={since}";
        else url += "&full_state=true";
        Console.WriteLine("Calling: " + url);
        var res = await _homeServer._httpClient.GetFromJsonAsync<SyncResult>(url);
        await _storageService.CacheStorageProvider.SaveObject(outFileName, res);
        return res;
    }
    
    public event EventHandler<SyncResult>? ;
}

public class SyncResult {
    [JsonPropertyName("next_batch")]
    public string NextBatch { get; set; }

    [JsonPropertyName("account_data")]
    public EventList AccountData { get; set; }

    [JsonPropertyName("presence")]
    public PresenceDataStructure Presence { get; set; }

    [JsonPropertyName("device_one_time_keys_count")]
    public Dictionary<string, int> DeviceOneTimeKeysCount { get; set; }

    [JsonPropertyName("rooms")]
    public RoomsDataStructure Rooms { get; set; }

    // supporting classes
    public class PresenceDataStructure {
        [JsonPropertyName("events")]
        public List<StateEventResponse<PresenceStateEventData>> Events { get; set; }
    }

    public class RoomsDataStructure {
        [JsonPropertyName("join")]
        public Dictionary<string, JoinedRoomDataStructure> Join { get; set; }

        [JsonPropertyName("invite")]
        public Dictionary<string, InvitedRoomDataStructure> Invite { get; set; }
        
        public class JoinedRoomDataStructure {
            [JsonPropertyName("timeline")]
            public TimelineDataStructure Timeline { get; set; }

            [JsonPropertyName("state")]
            public EventList State { get; set; }

            [JsonPropertyName("account_data")]
            public EventList AccountData { get; set; }

            [JsonPropertyName("ephemeral")]
            public EventList Ephemeral { get; set; }

            [JsonPropertyName("unread_notifications")]
            public UnreadNotificationsDataStructure UnreadNotifications { get; set; }
            
            [JsonPropertyName("summary")]
            public SummaryDataStructure Summary { get; set; }

            public class TimelineDataStructure {
                [JsonPropertyName("events")]
                public List<StateEventResponse> Events { get; set; }

                [JsonPropertyName("prev_batch")]
                public string PrevBatch { get; set; }

                [JsonPropertyName("limited")]
                public bool Limited { get; set; }
            }

            public class UnreadNotificationsDataStructure {
                [JsonPropertyName("notification_count")]
                public int NotificationCount { get; set; }

                [JsonPropertyName("highlight_count")]
                public int HighlightCount { get; set; }
            }
            
            public class SummaryDataStructure {
                [JsonPropertyName("m.heroes")]
                public List<string> Heroes { get; set; }
                [JsonPropertyName("m.invited_member_count")]
                public int InvitedMemberCount { get; set; }
                [JsonPropertyName("m.joined_member_count")]
                public int JoinedMemberCount { get; set; }
            }
        }

        public class InvitedRoomDataStructure {
            [JsonPropertyName("invite_state")]
            public EventList InviteState { get; set; }
        }
    }
}

public class EventList {
    [JsonPropertyName("events")]
    public List<StateEventResponse> Events { get; set; }
}