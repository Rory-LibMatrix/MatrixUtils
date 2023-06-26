using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

[MatrixEvent(EventName = "m.room.message")]
public class MessageEventData : IStateEventType {
    [JsonPropertyName("body")]
    public string Body { get; set; }
    [JsonPropertyName("msgtype")]
    public string MessageType { get; set; } = "m.notice";

    [JsonPropertyName("formatted_body")]
    public string FormattedBody { get; set; }

    [JsonPropertyName("format")]
    public string Format { get; set; }
}