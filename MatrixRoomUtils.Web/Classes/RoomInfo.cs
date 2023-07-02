using MatrixRoomUtils.Core;
using MatrixRoomUtils.Core.Responses;
using MatrixRoomUtils.Core.RoomTypes;

namespace MatrixRoomUtils.Web.Classes; 

public class RoomInfo {
    public GenericRoom Room { get; set; }
    public List<StateEventResponse?> StateEvents { get; init; } = new();
    
    public async Task<StateEventResponse?> GetStateEvent(string type, string stateKey = "") {
        var @event = StateEvents.FirstOrDefault(x => x.Type == type && x.StateKey == stateKey);
        if (@event is not null) return @event;
        @event = new StateEventResponse() {
            RoomId = Room.RoomId,
            Type = type,
            StateKey = stateKey,
        };
        try {
            @event.TypedContent = await Room.GetStateAsync<object>(type, stateKey);
        }
        catch (MatrixException e) {
            if (e is { ErrorCode: "M_NOT_FOUND" }) @event.TypedContent = default!;
            else throw;
        }
        StateEvents.Add(@event);
        return @event;
    }
}