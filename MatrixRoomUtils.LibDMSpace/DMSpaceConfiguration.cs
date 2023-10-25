using System.Text.Json.Serialization;

namespace MatrixRoomUtils.LibDMSpace;

//gay.rory.dm_space
public class DMSpaceConfiguration {
    public const string EventId = "gay.rory.dm_space";

    [JsonPropertyName("dm_space_id")]
    public string? DMSpaceId { get; set; }

}