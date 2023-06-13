using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core;

public class StateEvent {
    [JsonPropertyName("content")]
    public dynamic Content { get; set; } = new { };

    [JsonPropertyName("state_key")]
    public string StateKey { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("replaces_state")]
    public string? ReplacesState { get; set; }

    //extra properties
    [JsonIgnore]
    public JsonNode ContentAsJsonNode {
        get => JsonSerializer.SerializeToNode(Content);
        set => Content = value;
    }

    public string dtype {
        get {
            var res = GetType().Name switch {
                "StateEvent`1" => $"StateEvent<{Content.GetType().Name}>",
                _ => GetType().Name
            };
            return res;
        }
    }

    public StateEvent<T> As<T>() where T : class => (StateEvent<T>)this;
}

public class StateEvent<T> : StateEvent where T : class {
    public StateEvent() {
        //import base content if not an empty object
        if (base.Content.GetType() == typeof(T)) {
            Console.WriteLine($"StateEvent<{typeof(T)}> created with base content of type {base.Content.GetType()}. Importing base content.");
            Content = base.Content;
        }
    }

    [JsonPropertyName("content")]
    public new T Content { get; set; }
}