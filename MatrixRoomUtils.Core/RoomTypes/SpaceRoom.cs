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

    private static SemaphoreSlim _semaphore = new(1, 1);
    public async IAsyncEnumerable<GenericRoom> GetRoomsAsync(bool includeRemoved = false) {
        await _semaphore.WaitAsync();
        var rooms = new List<GenericRoom>();
        var state = GetFullStateAsync();
        await foreach (var stateEvent in state) {
            if (stateEvent.Type != "m.space.child") continue;
            if (stateEvent.RawContent.ToJson() != "{}" || includeRemoved)
                yield return await _homeServer.GetRoom(stateEvent.StateKey);
        }
        _semaphore.Release();
    }
}