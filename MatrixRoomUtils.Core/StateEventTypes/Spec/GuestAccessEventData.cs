using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes.Spec;

[MatrixEvent(EventName = "m.room.guest_access")]
public class GuestAccessEventData : IStateEventType {
    [JsonPropertyName("guest_access")]
    public string GuestAccess { get; set; }

    public bool IsGuestAccessEnabled {
        get => GuestAccess == "can_join";
        set => GuestAccess = value ? "can_join" : "forbidden";
    }
}