using System.Text.Json.Serialization;
using LibMatrix.EventTypes;
using LibMatrix.Interfaces;

namespace MatrixRoomUtils.LibDMSpace.StateEvents;

[MatrixEvent(EventName = EventId)]
public class DMSpaceInfo : TimelineEventContent {
    public const string EventId = "gay.rory.dm_space_info";

    [JsonPropertyName("is_layered")]
    public bool LayerByUser { get; set; }

}
