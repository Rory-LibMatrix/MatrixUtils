using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core;

public class StateEventResponse
{
    [JsonPropertyName("Content")]
    public dynamic Content { get; set; }
    [JsonPropertyName("origin_server_ts")]
    public long OriginServerTs { get; set; }
    [JsonPropertyName("RoomId")]
    public string RoomId { get; set; }
    [JsonPropertyName("Sender")]
    public string Sender { get; set; }
    [JsonPropertyName("StateKey")]
    public string StateKey { get; set; }
    [JsonPropertyName("Type")]
    public string Type { get; set; }
    [JsonPropertyName("Unsigned")]
    public dynamic Unsigned { get; set; }
    [JsonPropertyName("EventId")]
    public string EventId { get; set; }
    [JsonPropertyName("UserId")]
    public string UserId { get; set; }
    [JsonPropertyName("ReplacesState")]
    public string ReplacesState { get; set; }
    [JsonPropertyName("PrevContent")]
    public dynamic PrevContent { get; set; }
}

public class StateEventResponse<T> : StateEventResponse where T : class
{
    public T content { get; set; }
}