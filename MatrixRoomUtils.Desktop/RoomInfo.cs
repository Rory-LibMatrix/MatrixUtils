using LibMatrix;
using LibMatrix.EventTypes;
using LibMatrix.Interfaces;
using LibMatrix.Responses;
using LibMatrix.RoomTypes;

namespace MatrixRoomUtils.Desktop;

public class RoomInfo {
    public RoomInfo() { }

    public RoomInfo(GenericRoom room) {
        Room = room;
    }

    public GenericRoom Room { get; set; }
    public List<StateEventResponse?> StateEvents { get; init; } = new();

    public async Task<StateEventResponse?> GetStateEvent(string type, string stateKey = "") {
        var @event = StateEvents.FirstOrDefault(x => x.Type == type && x.StateKey == stateKey);
        if (@event is not null) return @event;
        @event = new StateEventResponse {
            RoomId = Room.RoomId,
            Type = type,
            StateKey = stateKey,
            Sender = null, //TODO: implement
            EventId = null
        };
        try {
            @event.TypedContent = await Room.GetStateAsync<EventContent>(type, stateKey);
        }
        catch (MatrixException e) {
            if (e is { ErrorCode: "M_NOT_FOUND" }) @event.TypedContent = default!;
            else throw;
        }

        StateEvents.Add(@event);
        return @event;
    }
}
