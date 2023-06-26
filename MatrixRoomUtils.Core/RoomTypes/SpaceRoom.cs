using System.Text.Json;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;
using MatrixRoomUtils.Core.Responses;

namespace MatrixRoomUtils.Core.RoomTypes;

public class SpaceRoom : GenericRoom {
    private readonly AuthenticatedHomeServer _homeServer;
    private readonly GenericRoom _room;
    public SpaceRoom(AuthenticatedHomeServer homeServer, string roomId) : base(homeServer, roomId) {
        _homeServer = homeServer;
    }

    public async Task<List<GenericRoom>> GetRoomsAsync(bool includeRemoved = false) {
        var rooms = new List<GenericRoom>();
        var state = await GetStateAsync("");
        if (state != null) {
            var states = state.Value.Deserialize<StateEventResponse[]>()!;
            foreach (var stateEvent in states.Where(x => x.Type == "m.space.child")) {
                var roomId = stateEvent.StateKey;
                if(stateEvent.TypedContent.ToJson() != "{}" || includeRemoved)
                    rooms.Add(await _homeServer.GetRoom(roomId));
            }
        }

        return rooms;
    }
}