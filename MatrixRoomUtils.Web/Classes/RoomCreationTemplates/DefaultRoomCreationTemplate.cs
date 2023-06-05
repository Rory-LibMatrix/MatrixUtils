using MatrixRoomUtils.Core;
using MatrixRoomUtils.Core.Responses;

namespace MatrixRoomUtils.Web.Classes.RoomCreationTemplates;

public class DefaultRoomCreationTemplate : IRoomCreationTemplate
{
    public string Name => "Default";
    public CreateRoomRequest CreateRoomRequest
    {
        get
        {
            return new()
            {
                Name = "My new room",
                RoomAliasName = "myroom",
                InitialState = new()
                {
                    new()
                    {
                        Type = "m.room.history_visibility",
                        Content = new
                        {
                            history_visibility = "world_readable"
                        }
                    },
                    new StateEvent<Pages.RoomManager.RoomManagerCreateRoom.GuestAccessContent>
                    {
                        Type = "m.room.guest_access",
                        Content = new()
                        {
                            GuestAccess = "can_join"
                        }
                    },
                    new()
                    {
                        Type = "m.room.join_rules",
                        Content = new
                        {
                            join_rule = "public"
                        }
                    },
                    new()
                    {
                        Type = "m.room.server_acl",
                        Content = new
                        {
                            allow = new[] { "*" },
                            deny = Array.Empty<string>(),
                            allow_ip_literals = false
                        }
                    },
                    new()
                    {
                        Type = "m.room.avatar",
                        Content = new
                        {
                            url = "mxc://feline.support/UKNhEyrVsrAbYteVvZloZcFj"
                        }
                    }
                },
                Visibility = "public",
                PowerLevelContentOverride = new()
                {
                    UsersDefault = 0,
                    EventsDefault = 100,
                    StateDefault = 50,
                    Invite = 0,
                    Redact = 50,
                    Kick = 50,
                    Ban = 50,
                    NotificationsPl = new()
                    {
                        Room = 50
                    },
                    Events = new()
                    {
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
                    Users = new()
                    {
                        { RuntimeCache.CurrentHomeServer.UserId, 100 },
                    },
                },
                CreationContent = new()
                {
                    {
                        "type", null
                    }
                }
            };
        }
    }
}