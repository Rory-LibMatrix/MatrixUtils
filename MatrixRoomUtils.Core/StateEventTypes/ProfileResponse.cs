using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;

namespace MatrixRoomUtils.Core.StateEventTypes;

[MatrixEvent(EventName = "m.room.member")]
public class ProfileResponse {
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; } = "";

    [JsonPropertyName("displayname")]
    public string? DisplayName { get; set; } = "";
}