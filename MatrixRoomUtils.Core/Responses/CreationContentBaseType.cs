using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core.Responses;

public class CreationContentBaseType {
    private readonly CreateRoomRequest createRoomRequest;

    public CreationContentBaseType(CreateRoomRequest createRoomRequest) => this.createRoomRequest = createRoomRequest;

    [JsonPropertyName("type")]
    public string Type {
        get => (string)createRoomRequest.CreationContent["type"];
        set {
            if (value is "null" or "") createRoomRequest.CreationContent.Remove("type");
            else createRoomRequest.CreationContent["type"] = value;
        }
    }
}