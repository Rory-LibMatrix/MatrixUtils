using LibMatrix;
using LibMatrix.EventTypes.Interop.Draupnir;
using LibMatrix.EventTypes.Spec.State.Policy;
using LibMatrix.Homeservers;
using LibMatrix.Services;
using SpawnDev.BlazorJS.WebWorkers;

namespace MatrixUtils.Web.Pages.Rooms;

public partial class PolicyList {
#region Draupnir interop

    private SemaphoreSlim ss = new(16, 16);

    private async Task DraupnirKickMatching(MatrixEventResponse policy) {
        try {
            var content = policy.TypedContent! as PolicyRuleEventContent;
            if (content is null) return;
            if (string.IsNullOrWhiteSpace(content.Entity)) return;

            var data = await Homeserver.GetAccountDataAsync<DraupnirProtectedRoomsData>(DraupnirProtectedRoomsData.EventId);
            var rooms = data.Rooms.Select(Homeserver.GetRoom).ToList();

            ActiveKicks.Add(policy, rooms.Count);
            StateHasChanged();
            await Task.Delay(500);

            // for (int i = 0; i < 12; i++) {
            // _ = WebWorkerService.TaskPool.Invoke(WasteCpu);
            // }

            // static async Task runKicks(string roomId, PolicyRuleEventContent content) {
            //     Console.WriteLine($"Checking {roomId}...");
            //     // Console.WriteLine($"Checking {room.RoomId}...");
            //     //
            //     // try {
            //     //     var members = await room.GetMembersListAsync();
            //     //     foreach (var member in members) {
            //     //         var membership = member.ContentAs<RoomMemberEventContent>();
            //     //         if (member.StateKey == room.Homeserver.WhoAmI.UserId) continue;
            //     //         if (membership?.Membership is "leave" or "ban") continue;
            //     //
            //     //         if (content.EntityMatches(member.StateKey!))
            //     //             // await room.KickAsync(member.StateKey, content.Reason ?? "No reason given");
            //     //             Console.WriteLine($"Would kick {member.StateKey} from {room.RoomId} (EntityMatches)");
            //     //     }
            //     // }
            //     // finally {
            //     //     Console.WriteLine($"Finished checking {room.RoomId}...");
            //     // }
            // }
            //
            // try {
            //     var tasks = rooms.Select(room => WebWorkerService.TaskPool.Invoke(runKicks, room.RoomId, content)).ToList();
            //
            //     await Task.WhenAll(tasks);
            // }
            // catch (Exception e) {
            //     Console.WriteLine(e);
            // }

            await NastyInternalsPleaseIgnore.ExecuteKickWithWasmWorkers(WebWorkerService, Homeserver, policy, data.Rooms);
            // await Task.Run(async () => {
            //     foreach (var room in rooms) {
            //         try {
            //             Console.WriteLine($"Checking {room.RoomId}...");
            //             var members = await room.GetMembersListAsync();
            //             foreach (var member in members) {
            //                 var membership = member.ContentAs<RoomMemberEventContent>();
            //                 if (member.StateKey == room.Homeserver.WhoAmI.UserId) continue;
            //                 if (membership?.Membership is "leave" or "ban") continue;
            //
            //                 if (content.EntityMatches(member.StateKey!))
            //                     // await room.KickAsync(member.StateKey, content.Reason ?? "No reason given");
            //                     Console.WriteLine($"Would kick {member.StateKey} from {room.RoomId} (EntityMatches)");
            //             }
            //             ActiveKicks[policy]--;
            //             StateHasChanged();
            //         }
            //         finally {
            //             Console.WriteLine($"Finished checking {room.RoomId}...");
            //         }
            //     }
            // });
        }
        finally {
            ActiveKicks.Remove(policy);
            StateHasChanged();
            await Task.Delay(500);
        }
    }

#region Nasty, nasty internals, please ignore!

    private static class NastyInternalsPleaseIgnore {
        public static async Task ExecuteKickWithWasmWorkers(WebWorkerService workerService, AuthenticatedHomeserverGeneric hs, MatrixEventResponse evt, List<string> roomIds) {
            try {
                // var tasks = roomIds.Select(roomId => workerService.TaskPool.Invoke(ExecuteKickInternal, hs.WellKnownUris.Client, hs.AccessToken, roomId, content.Entity)).ToList();
                var tasks = roomIds.Select(roomId => workerService.TaskPool.Invoke(ExecuteKickInternal2, hs.WellKnownUris, hs.AccessToken, roomId, evt)).ToList();
                // workerService.TaskPool.Invoke(ExecuteKickInternal, hs.BaseUrl, hs.AccessToken, roomIds, content.Entity);
                await Task.WhenAll(tasks);
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private static async Task ExecuteKickInternal(string homeserverBaseUrl, string accessToken, string roomId, string entity) {
            try {
                Console.WriteLine("args: " + string.Join(", ", homeserverBaseUrl, accessToken, roomId, entity));
                Console.WriteLine($"Checking {roomId}...");
                var hs = new AuthenticatedHomeserverGeneric(homeserverBaseUrl, new() { Client = homeserverBaseUrl }, null, accessToken);
                Console.WriteLine($"Got HS...");
                var room = hs.GetRoom(roomId);
                Console.WriteLine($"Got room...");
                var members = await room.GetMembersListAsync();
                Console.WriteLine($"Got members...");
                // foreach (var member in members) {
                //     var membership = member.ContentAs<RoomMemberEventContent>();
                //     if (member.StateKey == hs.WhoAmI.UserId) continue;
                //     if (membership?.Membership is "leave" or "ban") continue;
                //
                //     if (entity == member.StateKey)
                //         // await room.KickAsync(member.StateKey, content.Reason ?? "No reason given");
                //         Console.WriteLine($"Would kick {member.StateKey} from {room.RoomId} (EntityMatches)");
                // }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private async static Task ExecuteKickInternal2(HomeserverResolverService.WellKnownUris wellKnownUris, string accessToken, string roomId, MatrixEventResponse policy) {
            Console.WriteLine($"Checking {roomId}...");
            Console.WriteLine(policy.EventId);
        }
    }

#endregion

#endregion
}