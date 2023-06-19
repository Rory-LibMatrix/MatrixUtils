using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core.StateEventTypes; 

public class PresenceStateEventData {
    [JsonPropertyName("presence")]
    public string Presence { get; set; }
    [JsonPropertyName("last_active_ago")]
    public long LastActiveAgo { get; set; }
    [JsonPropertyName("currently_active")]
    public bool CurrentlyActive { get; set; }
    [JsonPropertyName("status_msg")]
    public string StatusMessage { get; set; }
}