using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core.Filters;

public class SyncFilter {
    [JsonPropertyName("account_data")]
    public AccountDataFilter? AccountData { get; set; }

    [JsonPropertyName("presence")]
    public PresenceFilter? Presence { get; set; }

    [JsonPropertyName("room")]
    public RoomFilter? Room { get; set; }
}

public class PresenceFilter {
    [JsonPropertyName("not_types")]
    public List<string>? NotTypes { get; set; }
}

public class RoomFilter {
    [JsonPropertyName("account_data")]
    public AccountDataFilter? AccountData { get; set; }

    [JsonPropertyName("ephemeral")]
    public EphemeralFilter? Ephemeral { get; set; }

    public class EphemeralFilter {
        [JsonPropertyName("not_types")]
        public List<string>? NotTypes { get; set; }
    }

    [JsonPropertyName("state")]
    public StateFilter? State { get; set; }

    public class StateFilter {
        [JsonPropertyName("lazy_load_members")]
        public bool? LazyLoadMembers { get; set; }

        [JsonPropertyName("types")]
        public List<string>? Types { get; set; }
    }

    [JsonPropertyName("timeline")]
    public TimelineFilter? Timeline { get; set; }

    public class TimelineFilter {
        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        [JsonPropertyName("types")]
        public List<string>? Types { get; set; }

        [JsonPropertyName("not_types")]
        public List<string>? NotTypes { get; set; }
    }
}

public class AccountDataFilter {
    [JsonPropertyName("not_types")]
    public List<string>? NotTypes { get; set; }
}