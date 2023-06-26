using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.StateEventTypes; 

[MatrixEvent(EventName = "im.ponies.room_emotes")]
public class RoomEmotesEventData : IStateEventType {
    [JsonPropertyName("emoticons")]
    public Dictionary<string, EmoticonData>? Emoticons { get; set; }
    
    [JsonPropertyName("images")]
    public Dictionary<string, EmoticonData>? Images { get; set; }
    
    [JsonPropertyName("pack")]
    public PackInfo? Pack { get; set; }
    
    public class EmoticonData {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}

public class PackInfo {
    
}