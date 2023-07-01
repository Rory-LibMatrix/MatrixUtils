using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes.Spec; 

[MatrixEvent(EventName = "m.typing")]
public class RoomTypingEventData : IStateEventType {
    [JsonPropertyName("user_ids")]
    public string[]? UserIds { get; set; }
}