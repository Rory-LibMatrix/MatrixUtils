using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core.Filters;

public class SyncFilter {
    [JsonPropertyName("account_data")]
    public EventFilter? AccountData { get; set; }

    [JsonPropertyName("presence")]
    public EventFilter? Presence { get; set; }

    [JsonPropertyName("room")]
    public RoomFilter? Room { get; set; }

    public class RoomFilter {
        [JsonPropertyName("account_data")]
        public StateFilter? AccountData { get; set; }

        [JsonPropertyName("ephemeral")]
        public StateFilter? Ephemeral { get; set; }

        [JsonPropertyName("state")]
        public StateFilter? State { get; set; }

        [JsonPropertyName("timeline")]
        public StateFilter? Timeline { get; set; }


        public class StateFilter : EventFilter {
            [JsonPropertyName("contains_url")]
            public bool? ContainsUrl { get; set; }

            [JsonPropertyName("include_redundant_members")]
            public bool? IncludeRedundantMembers { get; set; }

            [JsonPropertyName("lazy_load_members")]
            public bool? LazyLoadMembers { get; set; }

            [JsonPropertyName("rooms")]
            public List<string>? Rooms { get; set; }

            [JsonPropertyName("not_rooms")]
            public List<string>? NotRooms { get; set; }

            [JsonPropertyName("unread_thread_notifications")]
            public bool? UnreadThreadNotifications { get; set; }
        }
    }

    public class EventFilter {
        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        [JsonPropertyName("types")]
        public List<string>? Types { get; set; }

        [JsonPropertyName("not_types")]
        public List<string>? NotTypes { get; set; }

        [JsonPropertyName("senders")]
        public List<string>? Senders { get; set; }

        [JsonPropertyName("not_senders")]
        public List<string>? NotSenders { get; set; }
    }
}
