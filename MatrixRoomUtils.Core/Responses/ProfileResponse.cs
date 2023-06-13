using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core.Responses;

public class ProfileResponse {
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; } = "";

    [JsonPropertyName("displayname")]
    public string? DisplayName { get; set; } = "";
}