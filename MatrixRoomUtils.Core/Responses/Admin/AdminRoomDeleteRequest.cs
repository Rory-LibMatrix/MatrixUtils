using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core.Responses.Admin; 

public class AdminRoomDeleteRequest {
    [JsonPropertyName("new_room_user_id")]
    public string? NewRoomUserId { get; set; }
    [JsonPropertyName("room_name")]
    public string? RoomName { get; set; }
    [JsonPropertyName("block")]
    public bool Block { get; set; }
    [JsonPropertyName("purge")]
    public bool Purge { get; set; }
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    [JsonPropertyName("force_purge")]
    public bool ForcePurge { get; set; }
}