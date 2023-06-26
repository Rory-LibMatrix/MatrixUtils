using MatrixRoomUtils.Core;
using MatrixRoomUtils.Core.Responses;

namespace MatrixRoomUtils.Bot.Interfaces;

public class CommandContext {
    public GenericRoom Room { get; set; }
    public StateEventResponse MessageEvent { get; set; }
    public string CommandName => (MessageEvent.TypedContent as MessageEventData).Body.Split(' ')[0][1..];
    public string[] Args => (MessageEvent.TypedContent as MessageEventData).Body.Split(' ')[1..];
}