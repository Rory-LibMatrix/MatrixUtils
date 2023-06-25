using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes;

[MatrixEvent(EventName = "m.room.member")]
public class MemberEventData : IStateEventType {
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("membership")]
    public string Membership { get; set; } = null!;

    [JsonPropertyName("displayname")]
    public string? Displayname { get; set; }

    [JsonPropertyName("is_direct")]
    public bool? IsDirect { get; set; }
    
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }
}