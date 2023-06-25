using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core;

public class StateEvent {
    public static List<Type> KnownStateEventTypes =
        new ClassCollector<IStateEventType>().ResolveFromAllAccessibleAssemblies();

    public object TypedContent {
        get => RawContent.Deserialize(GetType)!;
        set => RawContent = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(value));
    }

    [JsonPropertyName("state_key")]
    public string StateKey { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("replaces_state")]
    public string? ReplacesState { get; set; }

    [JsonPropertyName("content")]
    public JsonObject? RawContent { get; set; }

    public T1 GetContent<T1>() where T1 : IStateEventType {
        return RawContent.Deserialize<T1>();
    }

    [JsonIgnore]
    public Type GetType {
        get {
            var type = StateEvent.KnownStateEventTypes.FirstOrDefault(x =>
                x.GetCustomAttribute<MatrixEventAttribute>()?.EventName == Type);
            if (type == null) {
                Console.WriteLine($"Warning: unknown event type '{Type}'!");
                Console.WriteLine(RawContent.ToJson());
                return typeof(object);
            }

            RawContent.FindExtraJsonObjectFields(type);
            
            return type;
        }
    }

    //debug
    public string dtype {
        get {
            var res = GetType().Name switch {
                "StateEvent`1" => $"StateEvent",
                _ => GetType().Name
            };
            return res;
        }
    }

    public string cdtype => TypedContent.GetType().Name;
}