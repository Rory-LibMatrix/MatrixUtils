using System.Text.Json.Serialization;
using LibMatrix.EventTypes;
using LibMatrix.Interfaces;

namespace MatrixRoomUtils.LibDMSpace.StateEvents;

[MatrixEvent(EventName = EventId)]
public class DMRoomInfo : TimelineEventContent {
    public const string EventId = "gay.rory.dm_room_info";
    [JsonPropertyName("remote_users")]
    public List<string> RemoteUsers { get; set; }


}
