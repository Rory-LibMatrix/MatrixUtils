using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes;

[MatrixEvent(EventName = "m.room.history_visibility")]
public class HistoryVisibilityData : IStateEventType {
    public string HistoryVisibility { get; set; }
}