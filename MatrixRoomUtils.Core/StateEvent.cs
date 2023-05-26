using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core;

public class StateEvent
{
    [JsonPropertyName("content")]
    public dynamic Content { get; set; } = new{};
    [JsonPropertyName("state_key")]
    public string? StateKey { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("replaces_state")]
    public string? ReplacesState { get; set; }
    
    //extra properties
    [JsonIgnore]
    public JsonNode ContentAsJsonNode
    {
        get => JsonSerializer.SerializeToNode(Content);
        set => Content = value;
    }
}

public class StateEvent<T> : StateEvent where T : class
{
    public new T content { get; set; }
    
    
    [JsonIgnore]
    public new JsonNode ContentAsJsonNode
    {
        get => JsonSerializer.SerializeToNode(Content);
        set => Content = value.Deserialize<T>();
    }
}