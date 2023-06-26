using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes;

[MatrixEvent(EventName = "m.room.server_acl")]
public class ServerACLData : IStateEventType {
    [JsonPropertyName("allow")]
    public List<string> Allow { get; set; } // = null!;

    [JsonPropertyName("deny")]
    public List<string> Deny { get; set; } // = null!;

    [JsonPropertyName("allow_ip_literals")]
    public bool AllowIpLiterals { get; set; } // = false;
}