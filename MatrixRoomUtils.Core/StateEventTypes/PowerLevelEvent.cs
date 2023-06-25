using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes;

[MatrixEvent(EventName = "m.room.power_levels")]
public class PowerLevelEvent : IStateEventType {
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
    
    public class NotificationsPL {
        [JsonPropertyName("room")]
        public int Room { get; set; } = 50;
    }
}