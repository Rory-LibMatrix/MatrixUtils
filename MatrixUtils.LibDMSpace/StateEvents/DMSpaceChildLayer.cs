using System.Text.Json.Serialization;
using LibMatrix.EventTypes;
using LibMatrix.Interfaces;

namespace MatrixUtils.LibDMSpace.StateEvents;

[MatrixEvent(EventName = EventId)]
public class DMSpaceChildLayer : EventContent {
    public const string EventId = "gay.rory.dm_space_child_layer";

    [JsonPropertyName("space_id")]
    public string SpaceId { get; set; }
    
    [JsonPropertyName("override_name")]
    public string? OverrideName { get; set; }
    
    [JsonPropertyName("override_avatar")]
    public string? OverrideAvatar { get; set; }
    
}
