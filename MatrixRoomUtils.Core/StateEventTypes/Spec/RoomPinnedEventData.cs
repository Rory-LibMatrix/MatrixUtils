using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes; 

[MatrixEvent(EventName = "m.room.pinned_events")]
public class RoomPinnedEventData : IStateEventType {
    [JsonPropertyName("pinned")]
    public string[]? PinnedEvents { get; set; }
}