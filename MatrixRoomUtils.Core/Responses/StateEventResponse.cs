using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core.Responses;

public class StateEventResponse : StateEvent {
    [JsonPropertyName("origin_server_ts")]
    public ulong OriginServerTs { get; set; }

    [JsonPropertyName("room_id")]
    public string RoomId { get; set; }

    [JsonPropertyName("sender")]
    public string Sender { get; set; }

    [JsonPropertyName("unsigned")]
    public UnsignedData? Unsigned { get; set; }

    [JsonPropertyName("event_id")]
    public string EventId { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("replaces_state")]
    public string ReplacesState { get; set; }

    [JsonPropertyName("prev_content")]
    public dynamic PrevContent { get; set; }

    public class UnsignedData {
        [JsonPropertyName("age")]
        public ulong Age { get; set; }

        [JsonPropertyName("prev_content")]
        public dynamic? PrevContent { get; set; }

        [JsonPropertyName("redacted_because")]
        public dynamic? RedactedBecause { get; set; }

        [JsonPropertyName("transaction_id")]
        public string? TransactionId { get; set; }
    }
}

public class StateEventResponse<T> : StateEventResponse where T : class {
    [JsonPropertyName("content")]
    public T Content { get; set; }
}