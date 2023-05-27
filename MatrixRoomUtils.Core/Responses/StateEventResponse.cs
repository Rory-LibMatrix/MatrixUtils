using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core;

public class StateEventResponse
{
    [JsonPropertyName("content")]
    public dynamic Content { get; set; }
    [JsonPropertyName("origin_server_ts")]
    public long OriginServerTs { get; set; }
    [JsonPropertyName("room_id")]
    public string RoomId { get; set; }
    [JsonPropertyName("sender")]
    public string Sender { get; set; }
    [JsonPropertyName("state_key")]
    public string StateKey { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("unsigned")]
    public dynamic Unsigned { get; set; }
    [JsonPropertyName("event_id")]
    public string EventId { get; set; }
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    [JsonPropertyName("replaces_state")]
    public string ReplacesState { get; set; }
    [JsonPropertyName("prev_content")]
    public dynamic PrevContent { get; set; }
}

public class StateEventResponse<T> : StateEventResponse where T : class
{
    
    [JsonPropertyName("content")]
    public T Content { get; set; }
}