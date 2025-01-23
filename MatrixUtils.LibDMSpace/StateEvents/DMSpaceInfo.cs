using System.Text.Json.Serialization;
using LibMatrix.EventTypes;

namespace MatrixUtils.LibDMSpace.StateEvents;

[MatrixEvent(EventName = EventId)]
public class DMSpaceInfo : EventContent {
    public const string EventId = "gay.rory.dm_space_info";

    [JsonPropertyName("is_layered")]
    public bool LayerByUser { get; set; }

}
