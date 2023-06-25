using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core;

internal class UserIdAndReason {
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}