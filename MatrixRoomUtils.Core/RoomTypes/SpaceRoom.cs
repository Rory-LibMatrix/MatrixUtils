using System.Text.Json;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;
using MatrixRoomUtils.Core.Responses;

namespace MatrixRoomUtils.Core.RoomTypes;

public class SpaceRoom : Room {
    public SpaceRoom(HttpClient httpClient, string roomId) : base(httpClient, roomId) { }

    public async Task<List<Room>> GetRoomsAsync(bool includeRemoved = false) {
        var rooms = new List<Room>();
        var state = await GetStateAsync("");
        if (state != null) {
            var states = state.Value.Deserialize<StateEventResponse[]>()!;
            foreach (var stateEvent in states.Where(x => x.Type == "m.space.child")) {
                var roomId = stateEvent.StateKey;
                if(stateEvent.TypedContent.ToJson() != "{}" || includeRemoved)
                    rooms.Add(await RuntimeCache.CurrentHomeServer.GetRoom(roomId));
            }
        }

        return rooms;
    }
}