using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes.Spec;

[MatrixEvent(EventName = "m.room.power_levels")]
public class RoomPowerLevelEventData : IStateEventType {
    [JsonPropertyName("ban")]
    public int Ban { get; set; } // = 50;

    [JsonPropertyName("events_default")]
    public int EventsDefault { get; set; } // = 0;

    [JsonPropertyName("events")]
    public Dictionary<string, int> Events { get; set; } // = null!;

    [JsonPropertyName("invite")]
    public int Invite { get; set; } // = 50;

    [JsonPropertyName("kick")]
    public int Kick { get; set; } // = 50;

    [JsonPropertyName("notifications")]
    public NotificationsPL NotificationsPl { get; set; } // = null!;

    [JsonPropertyName("redact")]
    public int Redact { get; set; } // = 50;

    [JsonPropertyName("state_default")]
    public int StateDefault { get; set; } // = 50;

    [JsonPropertyName("users")]
    public Dictionary<string, int> Users { get; set; } // = null!;

    [JsonPropertyName("users_default")]
    public int UsersDefault { get; set; } // = 0;

    [Obsolete("Historical was a key related to MSC2716, a spec change on backfill that was dropped!", true)]
    [JsonIgnore]
    [JsonPropertyName("historical")]
    public int Historical { get; set; } // = 50;

    public class NotificationsPL {
        [JsonPropertyName("room")]
        public int Room { get; set; } = 50;
    }

    public bool IsUserAdmin(string userId) {
        return Users.TryGetValue(userId, out var level) && level >= Events.Max(x=>x.Value);
    }

    public bool UserHasPermission(string userId, string eventType) {
        return Users.TryGetValue(userId, out var level) && level >= Events.GetValueOrDefault(eventType, EventsDefault);
    }
}
