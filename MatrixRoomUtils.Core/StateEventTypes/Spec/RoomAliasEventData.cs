using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core;

[MatrixEvent(EventName = "m.room.alias")]
public class RoomAliasEventData : IStateEventType {
    [JsonPropertyName("aliases")]
    public List<string>? Aliases { get; set; }
}