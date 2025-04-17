using ArcaneLibs.Extensions;
using LibMatrix;
using LibMatrix.EventTypes.Spec.State.RoomInfo;
using LibMatrix.Homeservers;
using LibMatrix.Responses;
using LibMatrix.RoomTypes;
using MatrixUtils.LibDMSpace.StateEvents;

namespace MatrixUtils.LibDMSpace;

public class DMSpaceRoom(AuthenticatedHomeserverGeneric homeserver, string roomId) : SpaceRoom(homeserver, roomId) {
    private readonly GenericRoom _room;

    // ReSharper disable once InconsistentNaming
    public async Task<DMSpaceInfo?> GetDMSpaceInfo() {
        return await GetStateOrNullAsync<DMSpaceInfo>(DMSpaceInfo.EventId);
    }

    public async Task<Dictionary<string, List<string>>?> ExportNativeDMs() {
        var state = GetFullStateAsync();
        var mdirect = new Dictionary<string, List<string>>();
        await foreach (var stateEvent in state) { }

        return mdirect;
    }

    public async Task ImportNativeDMs() {
        var dmSpaceInfo = await GetDMSpaceInfo();
        if (dmSpaceInfo is null) throw new NullReferenceException("DM Space is not configured!");
        if (dmSpaceInfo.LayerByUser)
            await ImportNativeDMsIntoLayers();
        else await ImportNativeDMsWithoutLayers();
    }

    public async Task<List<StateEventResponse>> GetAllActiveLayersAsync() {
        var state = await GetFullStateAsListAsync();
        return state.Where(x => x.Type == DMSpaceChildLayer.EventId && x.RawContent.ContainsKey("space_id")).ToList();
    }

#region Import Native DMs

    private async Task ImportNativeDMsWithoutLayers() {
        var mdirect = await homeserver.GetAccountDataAsync<Dictionary<string, List<string>>>("m.direct");
        foreach (var (userId, dmRooms) in mdirect) {
            foreach (var roomid in dmRooms) {
                var dri = new DMRoomInfo() {
                    AttributedUser = userId
                };
                await SendStateEventAsync(DMRoomInfo.EventId, roomid, dri);
                await AddChildAsync(Homeserver.GetRoom(roomid));
            }
        }
    }

    private async Task ImportNativeDMsIntoLayers() {
        var mdirect = await homeserver.GetAccountDataAsync<Dictionary<string, List<string>>>("m.direct");
        var layerTasks = mdirect.Select(async entry => {
            var (userId, dmRooms) = entry;
            DMSpaceChildLayer? layer = await GetStateOrNullAsync<DMSpaceChildLayer>(DMSpaceChildLayer.EventId, userId.UrlEncode()) ?? await CreateLayer(userId);
            return (entry, layer);
        }).ToAsyncEnumerable();

        await foreach (var ((userId, dmRooms), layer) in layerTasks) {
            var space = Homeserver.GetRoom(layer.SpaceId).AsSpace();
            foreach (var roomid in dmRooms) {
                var dri = new DMRoomInfo() {
                    AttributedUser = userId
                };
                await space.SendStateEventAsync(DMRoomInfo.EventId, roomid, dri);
                await space.AddChildAsync(Homeserver.GetRoom(roomid));
            }

            await UpdateLayer(layer, userId);
        }

        // ensure all spaces are linked
        Console.WriteLine("Ensuring all child layers are inside space...");
        var layerAssuranceTasks = (await GetFullStateAsListAsync())
            .Where(x => x.Type == DMSpaceChildLayer.EventId && (x.RawContent?.ContainsKey("space_id") ?? false))
            .Select(async layer => {
                var content = layer.TypedContent as DMSpaceChildLayer;
                var space = homeserver.GetRoom(content!.SpaceId);
                try {
                    var state = await space.GetFullStateAsListAsync();
                    if (!state.Any(e => e.Type == DMRoomInfo.EventId)) throw new Exception();
                }
                catch {
                    await homeserver.JoinRoomAsync(content!.SpaceId);
                }

                return AddChildAsync(space);
            });
        await Task.WhenAll(layerAssuranceTasks);
        Console.WriteLine("All child layers should be inside of space, if not, something went horribly wrong!");
    }

    private async Task<DMSpaceChildLayer> CreateLayer(string userId) {
        var childCreateRequest = CreateRoomRequest.CreatePrivate(homeserver, userId);
        childCreateRequest.CreationContent["type"] = "m.space";

        var layer = new DMSpaceChildLayer() {
            SpaceId = (await homeserver.CreateRoom(childCreateRequest)).RoomId
        };

        await SendStateEventAsync(DMSpaceChildLayer.EventId, userId[1..], layer);
        await AddChildAsync(Homeserver.GetRoom(layer.SpaceId));
        return layer;
    }

    private async Task UpdateLayerProfilesAsync() {
        var layers = await GetAllActiveLayersAsync();
        var getProfileTasks = layers.Select(async x => {
            UserProfileResponse? profile = null;
            try {
                return (x, profile);
            }
            catch {
                return (x, null);
            }

        }).ToAsyncEnumerable();
        await foreach (var (layer, profile) in getProfileTasks) {
            if (profile is null) continue;
            var layerContent = layer.TypedContent as DMSpaceChildLayer;
            var space = Homeserver.GetRoom(layerContent!.SpaceId).AsSpace();

            try {
                await space.SendStateEventAsync(RoomAvatarEventContent.EventId, "", new RoomAvatarEventContent() {
                    Url = layerContent.OverrideAvatar ?? profile?.AvatarUrl
                });
                await space.SendStateEventAsync(RoomNameEventContent.EventId, "", new RoomNameEventContent() {
                    Name = layerContent.OverrideName ?? profile?.DisplayName ?? "@" + layer.StateKey.UrlDecode()
                });
            }
            catch (MatrixException e) {
                Console.WriteLine("Failed to update space: {0}", e);
            }
        }
    }

    private async Task UpdateLayer(DMSpaceChildLayer layer, string mxid) {
        UserProfileResponse? profile = null;
        var space = Homeserver.GetRoom(layer.SpaceId).AsSpace();

        if (string.IsNullOrWhiteSpace(layer.OverrideAvatar) || string.IsNullOrWhiteSpace(layer.OverrideName)) {
            try {
                profile = await homeserver.GetProfileAsync(mxid);
            }
            catch (MatrixException e) {
                // if (e.ErrorCode != "M_NOT_FOUND") throw;
                Console.Error.WriteLine(e);
            }
        }

        try {
            await space.SendStateEventAsync(RoomAvatarEventContent.EventId, "", new RoomAvatarEventContent() {
                Url = layer.OverrideAvatar ?? profile?.AvatarUrl
            });
            await space.SendStateEventAsync(RoomNameEventContent.EventId, "", new RoomNameEventContent() {
                Name = layer.OverrideName ?? profile?.DisplayName ?? mxid
            });
        }
        catch (MatrixException e) {
            Console.WriteLine("Failed to update space: {0}", e);
        }
    }

    public async Task DisbandDMSpace() {
        var state = await GetFullStateAsListAsync();
        var leaveTasks = state.Select(async x => {
            if (x.Type != DMSpaceChildLayer.EventId) return;
            var content = x.TypedContent as DMSpaceChildLayer;
            if (content?.SpaceId is null) return;
            var space = homeserver.GetRoom(content.SpaceId);
            try {
                await space.LeaveAsync();
            }
            catch {
                // might not be in room, doesnt matter
            }
        });

        await LeaveAsync();

        await Task.WhenAll(leaveTasks);
    }

#endregion
}