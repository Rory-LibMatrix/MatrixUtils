using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core;

public class CreateEvent {
    [JsonPropertyName("creator")]
    public string Creator { get; set; }

    [JsonPropertyName("room_version")]
    public string RoomVersion { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("predecessor")]
    public object? Predecessor { get; set; }

    [JsonPropertyName("m.federate")]
    public bool Federate { get; set; }
}