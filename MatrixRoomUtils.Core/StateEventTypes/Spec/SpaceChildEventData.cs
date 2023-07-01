using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes.Spec; 

[MatrixEvent(EventName = "m.space.child")]
public class SpaceChildEventData : IStateEventType {
    [JsonPropertyName("auto_join")]
    public bool? AutoJoin { get; set; }
    [JsonPropertyName("via")]
    public string[]? Via { get; set; }
    [JsonPropertyName("suggested")]
    public bool? Suggested { get; set; }
}