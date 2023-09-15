using System.Text.Json.Nodes;
using LibMatrix;
using LibMatrix.Responses;
using LibMatrix.StateEventTypes.Spec;

namespace MatrixRoomUtils.Web.Classes.RoomCreationTemplates;

public class DefaultRoomCreationTemplate : IRoomCreationTemplate {
    public string Name => "Default";

    public CreateRoomRequest CreateRoomRequest =>
        new() {
            Name = "My new room",
            RoomAliasName = "myroom",
            InitialState = new List<StateEvent> {
                new() {
                    Type = "m.room.history_visibility",
                    TypedContent = new HistoryVisibilityEventContent() {
                        HistoryVisibility = "world_readable"
                    }
                },
                new() {
                    Type = "m.room.guest_access",
                    TypedContent = new GuestAccessEventContent {
                        GuestAccess = "can_join"
                    }
                },
                new() {
                    Type = "m.room.join_rules",
                    TypedContent = new JoinRulesEventContent {
                        JoinRule = "public"
                    }
                },
                new() {
                    Type = "m.room.server_acl",
                    TypedContent = new ServerACLEventContent() {
                        Allow = new List<string>() { "*" },
                        Deny = new List<string>(),
                        AllowIpLiterals = false
                    }
                },
                new() {
                    Type = "m.room.avatar",
                    TypedContent = new RoomAvatarEventContent {
                        Url = "mxc://feline.support/UKNhEyrVsrAbYteVvZloZcFj"
                    }
                }
            },
            Visibility = "public",
            PowerLevelContentOverride = new RoomPowerLevelEventContent {
                UsersDefault = 0,
                EventsDefault = 100,
                StateDefault = 50,
                Invite = 0,
                Redact = 50,
                Kick = 50,
                Ban = 50,
                NotificationsPl = new RoomPowerLevelEventContent.NotificationsPL {
                    Room = 50
                },
                Events = new() {
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
                Users = new() {
                    // { RuntimeCache.CurrentHomeServer.UserId, 100 }
                    //TODO: re-implement this
                }
            },
            CreationContent = new JsonObject {
                {
                    "type", null
                }
            }
        };
}
