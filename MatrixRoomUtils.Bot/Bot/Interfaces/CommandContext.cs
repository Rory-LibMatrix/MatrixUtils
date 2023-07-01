using MatrixRoomUtils.Core.Responses;
using MatrixRoomUtils.Core.RoomTypes;
using MatrixRoomUtils.Core.StateEventTypes.Spec;

namespace MatrixRoomUtils.Bot.Bot.Interfaces;

public class CommandContext {
    public GenericRoom Room { get; set; }
    public StateEventResponse MessageEvent { get; set; }
    public string CommandName => (MessageEvent.TypedContent as RoomMessageEventData).Body.Split(' ')[0][1..];
    public string[] Args => (MessageEvent.TypedContent as RoomMessageEventData).Body.Split(' ')[1..];
}