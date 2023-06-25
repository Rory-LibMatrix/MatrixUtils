using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes; 

[MatrixEvent(EventName = "m.room.canonical_alias")]
public class CanonicalAliasEventData : IStateEventType {
    [JsonPropertyName("alias")]
    public string? Alias { get; set; }
}