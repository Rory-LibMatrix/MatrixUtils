using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core;

[MatrixEvent(EventName = "m.room.join_rules")]
public class JoinRulesEventData : IStateEventType {
    private static string Public = "public";
    private static string Invite = "invite";
    private static string Knock = "knock";

    [JsonPropertyName("join_rule")]
    public string JoinRule { get; set; }

    [JsonPropertyName("allow")]
    public List<string> Allow { get; set; }
}