using System.Text.Json.Serialization;
using LibMatrix.EventTypes;

namespace MatrixUtils.LibDMSpace.StateEvents;

[MatrixEvent(EventName = EventId)]
public class DMRoomInfo : EventContent {
    public const string EventId = "gay.rory.dm_room_info";
    // [JsonPropertyName("remote_users")]
    // public List<string> RemoteUsers { get; set; }

    [JsonPropertyName("attributed_user")]
    public string AttributedUser { get; set; }
    

}
