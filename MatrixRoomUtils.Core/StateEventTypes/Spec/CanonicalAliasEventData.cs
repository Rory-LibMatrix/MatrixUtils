using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes.Spec; 

[MatrixEvent(EventName = "m.room.canonical_alias")]
public class CanonicalAliasEventData : IStateEventType {
    [JsonPropertyName("alias")]
    public string? Alias { get; set; }
    [JsonPropertyName("alt_aliases")]
    public string[]? AltAliases { get; set; }
}