using System.Text.Json.Nodes;
using MatrixRoomUtils.Core;
using MatrixRoomUtils.Core.Responses;
using MatrixRoomUtils.Core.StateEventTypes;

namespace MatrixRoomUtils.Web.Classes.RoomCreationTemplates;

public class DefaultRoomCreationTemplate : IRoomCreationTemplate {
    public string Name => "Default";

    public CreateRoomRequest CreateRoomRequest =>
        new CreateRoomRequest {
            Name = "My new room",
            RoomAliasName = "myroom",
            InitialState = new List<StateEvent> {
                new() {
                    Type = "m.room.history_visibility",
                    Content = new {
                        history_visibility = "world_readable"
                    }
                },
                new StateEvent<GuestAccessData> {
                    Type = "m.room.guest_access",
                    Content = new GuestAccessData {
                        GuestAccess = "can_join"
                    }
                },
                new() {
                    Type = "m.room.join_rules",
                    Content = new {
                        join_rule = "public"
                    }
                },
                new() {
                    Type = "m.room.server_acl",
                    Content = new {
                        allow = new[] { "*" },
                        deny = Array.Empty<string>(),
                        allow_ip_literals = false
                    }
                },
                new() {
                    Type = "m.room.avatar",
                    Content = new {
                        url = "mxc://feline.support/UKNhEyrVsrAbYteVvZloZcFj"
                    }
                }
            },
            Visibility = "public",
            PowerLevelContentOverride = new PowerLevelEvent {
                UsersDefault = 0,
                EventsDefault = 100,
                StateDefault = 50,
                Invite = 0,
                Redact = 50,
                Kick = 50,
                Ban = 50,
                NotificationsPl = new NotificationsPL {
                    Room = 50
                },
                Events = new Dictionary<string, int> {
                    { "im.vector.modular.widgets", 50 },
                    { "io.element.voice_broadcast_info", 50 },
                    { "m.reaction", 100 },
                    { "m.room.avatar", 50 },
                    { "m.room.canonical_alias", 50 },
                    { "m.room.encryption", 100 },
                    { "m.room.history_visibility", 100 },
                    { "m.room.name", 50 },
                    { "m.room.pinned_events", 50 },
                    { "m.room.power_levels", 100 },
                    { "m.room.redaction", 100 },
                    { "m.room.server_acl", 100 },
                    { "m.room.tombstone", 100 },
                    { "m.room.topic", 50 },
                    { "m.space.child", 50 },
                    { "org.matrix.msc3401.call", 50 },
                    { "org.matrix.msc3401.call.member", 50 }
                },
                Users = new Dictionary<string, int> {
                    { RuntimeCache.CurrentHomeServer.UserId, 100 }
                }
            },
            CreationContent = new JsonObject {
                {
                    "type", null
                }
            }
        };
}