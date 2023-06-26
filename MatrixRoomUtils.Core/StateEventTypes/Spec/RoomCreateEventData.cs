using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes; 

[MatrixEvent(EventName = "m.room.create")]
public class RoomCreateEventData : IStateEventType {
    [JsonPropertyName("room_version")]
    public string? RoomVersion { get; set; }
    [JsonPropertyName("creator")]
    public string? Creator { get; set; }
    [JsonPropertyName("m.federate")]
    public bool? Federate { get; set; }
    [JsonPropertyName("predecessor")]
    public RoomCreatePredecessor? Predecessor { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    public class RoomCreatePredecessor { }
}