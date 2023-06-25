using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core;

public class EventIdResponse {
    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = null!;
}