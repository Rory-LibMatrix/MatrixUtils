using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Interfaces;
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

    public async Task<SyncResult?> Sync(string? since = null, CancellationToken? cancellationToken = null) {
        var outFileName = "sync-" +
                          (await _storageService.CacheStorageProvider.GetAllKeys()).Count(x => x.StartsWith("sync")) +
                          ".json";
        var url = "/_matrix/client/v3/sync?timeout=30000&set_presence=online";
        if (!string.IsNullOrWhiteSpace(since)) url += $"&since={since}";
        else url += "&full_state=true";
        Console.WriteLine("Calling: " + url);
        try {
            var res = await _homeServer._httpClient.GetFromJsonAsync<SyncResult>(url,
                cancellationToken: cancellationToken ?? CancellationToken.None);
            await _storageService.CacheStorageProvider.SaveObject(outFileName, res);
            Console.WriteLine($"Wrote file: {outFileName}");
            return res;
        }
        catch (TaskCanceledException) {
            Console.WriteLine("Sync cancelled!");
        }
        catch (Exception e) {
            Console.WriteLine(e);
        }
        return null;
    }

    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    public async Task RunSyncLoop(CancellationToken? cancellationToken = null, bool skipInitialSyncEvents = true) {
        SyncResult? sync = null;
        while (cancellationToken is null || !cancellationToken.Value.IsCancellationRequested) {
            sync = await Sync(sync?.NextBatch, cancellationToken);
            Console.WriteLine($"Got sync, next batch: {sync?.NextBatch}!");
            if (sync == null) continue;
            if (sync.Rooms is { Invite.Count: > 0 }) {
                foreach (var roomInvite in sync.Rooms.Invite) {
                    Console.WriteLine(roomInvite.Value.GetType().Name);
                    InviteReceived?.Invoke(this, roomInvite);
                }
            }

            if (sync.AccountData is { Events: { Count: > 0 } }) {
                foreach (var accountDataEvent in sync.AccountData.Events) {
                    AccountDataReceived?.Invoke(this, accountDataEvent);
                }
            }

            // Things that are skipped on the first sync
            if (skipInitialSyncEvents) {
                skipInitialSyncEvents = false;
                continue;
            }

            if (sync.Rooms is { Join.Count: > 0 }) {
                foreach (var updatedRoom in sync.Rooms.Join) {
                    foreach (var stateEventResponse in updatedRoom.Value.Timeline.Events) {
                        stateEventResponse.RoomId = updatedRoom.Key;
                        TimelineEventReceived?.Invoke(this, stateEventResponse);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Event fired when a room invite is received
    /// </summary>
    public event EventHandler<KeyValuePair<string, SyncResult.RoomsDataStructure.InvitedRoomDataStructure>>? InviteReceived;

    public event EventHandler<StateEventResponse>? TimelineEventReceived;
    public event EventHandler<StateEventResponse>? AccountDataReceived;
}

public class SyncResult {
    [JsonPropertyName("next_batch")]
    public string NextBatch { get; set; }

    [JsonPropertyName("account_data")]
    public EventList? AccountData { get; set; }

    [JsonPropertyName("presence")]
    public PresenceDataStructure? Presence { get; set; }

    [JsonPropertyName("device_one_time_keys_count")]
    public Dictionary<string, int> DeviceOneTimeKeysCount { get; set; }

    [JsonPropertyName("rooms")]
    public RoomsDataStructure? Rooms { get; set; }

    // supporting classes
    public class PresenceDataStructure {
        [JsonPropertyName("events")]
        public List<StateEventResponse> Events { get; set; }
    }

    public class RoomsDataStructure {
        [JsonPropertyName("join")]
        public Dictionary<string, JoinedRoomDataStructure>? Join { get; set; }

        [JsonPropertyName("invite")]
        public Dictionary<string, InvitedRoomDataStructure>? Invite { get; set; }

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