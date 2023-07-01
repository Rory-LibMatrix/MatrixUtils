using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes.Spec;

[MatrixEvent(EventName = "m.room.member")]
public class ProfileResponseEventData : IStateEventType {
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; } = "";

    [JsonPropertyName("displayname")]
    public string? DisplayName { get; set; } = "";
}