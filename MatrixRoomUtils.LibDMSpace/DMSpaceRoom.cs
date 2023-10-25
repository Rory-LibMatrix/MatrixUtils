using ArcaneLibs.Extensions;
using LibMatrix;
using LibMatrix.Homeservers;
using LibMatrix.RoomTypes;
using MatrixRoomUtils.LibDMSpace.StateEvents;

namespace MatrixRoomUtils.LibDMSpace;

public class DMSpaceRoom(AuthenticatedHomeserverGeneric homeserver, string roomId) : SpaceRoom(homeserver, roomId) {
    private readonly GenericRoom _room;

    public async Task<DMSpaceInfo?> GetDmSpaceInfo() {
        return await GetStateOrNullAsync<DMSpaceInfo>(DMSpaceInfo.EventId);
    }

    public async IAsyncEnumerable<GenericRoom> GetChildrenAsync(bool includeRemoved = false) {
        var rooms = new List<GenericRoom>();
        var state = GetFullStateAsync();
        await foreach (var stateEvent in state) {
            if (stateEvent!.Type != "m.space.child") continue;
            if (stateEvent.RawContent!.ToJson() != "{}" || includeRemoved)
                yield return homeserver.GetRoom(stateEvent.StateKey);
        }
    }

    public async Task<EventIdResponse> AddChildAsync(GenericRoom room) {
        var members = room.GetMembersAsync(true);
        Dictionary<string, int> memberCountByHs = new();
        await foreach (var member in members) {
            var server = member.StateKey.Split(':')[1];
            if (memberCountByHs.ContainsKey(server)) memberCountByHs[server]++;
            else memberCountByHs[server] = 1;
        }

        var resp = await SendStateEventAsync("m.space.child", room.RoomId, new {
            via = memberCountByHs
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .Take(10)
        });
        return resp;
    }

    public async Task ImportNativeDMs() {
        var dmSpaceInfo = await GetDmSpaceInfo();
        if (dmSpaceInfo is null) throw new NullReferenceException("DM Space is not configured!");
        if (dmSpaceInfo.LayerByUser)
            await ImportNativeDMsIntoLayers();
        else await ImportNativeDMsWithoutLayers();
    }

#region Import Native DMs

    private async Task ImportNativeDMsWithoutLayers() {
        var mdirect = await homeserver.GetAccountDataAsync<Dictionary<string, List<string>>>("m.direct");
        foreach (var (userId, dmRooms) in mdirect) {
            foreach (var roomid in dmRooms) {
                var dri = new DMRoomInfo() {
                    RemoteUsers = new() {
                        userId
                    }
                };
                // Add all DM room members
                var members = homeserver.GetRoom(roomid).GetMembersAsync();
                await foreach (var member in members)
                    if (member.StateKey != userId)
                        dri.RemoteUsers.Add(member.StateKey);
                // Remove members of DM space
                members = GetMembersAsync();
                await foreach (var member in members)
                    if (dri.RemoteUsers.Contains(member.StateKey))
                        dri.RemoteUsers.Remove(member.StateKey);
                await SendStateEventAsync(DMRoomInfo.EventId, roomid, dri);
            }
        }
    }

    private async Task ImportNativeDMsIntoLayers() {
        var mdirect = await homeserver.GetAccountDataAsync<Dictionary<string, List<string>>>("m.direct");
    }

#endregion
}