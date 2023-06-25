using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core;

public class JoinRules {
    private static string Public = "public";
    private static string Invite = "invite";
    private static string Knock = "knock";

    [JsonPropertyName("join_rule")]
    public string JoinRule { get; set; }

    [JsonPropertyName("allow")]
    public List<string> Allow { get; set; }
}