using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Interfaces;
using MatrixRoomUtils.Core.Responses;

namespace MatrixRoomUtils.Core;

public class MessagesResponse {
    [JsonPropertyName("start")]
    public string Start { get; set; }

    [JsonPropertyName("end")]
    public string? End { get; set; }

    [JsonPropertyName("chunk")]
    public List<StateEventResponse> Chunk { get; set; } = new();

    [JsonPropertyName("state")]
    public List<StateEventResponse> State { get; set; } = new();
}