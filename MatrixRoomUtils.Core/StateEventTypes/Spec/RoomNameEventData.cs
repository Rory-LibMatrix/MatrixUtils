using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes; 

[MatrixEvent(EventName = "m.room.name")]
public class RoomNameEventData : IStateEventType {
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}