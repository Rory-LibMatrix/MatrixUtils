using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes; 

[MatrixEvent(EventName = "m.room.topic")]
public class RoomTopicEventData : IStateEventType {
    [JsonPropertyName("topic")]
    public string? Topic { get; set; }
}