using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes;

[MatrixEvent(EventName = "m.room.history_visibility")]
public class HistoryVisibilityData : IStateEventType {
    [JsonPropertyName("history_visibility")]
    public string HistoryVisibility { get; set; }
}