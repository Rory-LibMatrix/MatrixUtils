using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;
using MatrixRoomUtils.Core.Responses;
using MatrixRoomUtils.Core.StateEventTypes;

namespace MatrixRoomUtils.Core;

public class StateEvent {
    public static List<Type> KnownStateEventTypes =
        new ClassCollector<IStateEventType>().ResolveFromAllAccessibleAssemblies();

    public object TypedContent {
        get {
            try {
                return RawContent.Deserialize(GetType)!;
            }
            catch (JsonException e) {
                Console.WriteLine(e);
                Console.WriteLine("Content:\n"+ObjectExtensions.ToJson(RawContent));
            }
            return null;
        }
        set => RawContent = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(value));
    }

    [JsonPropertyName("state_key")]
    public string StateKey { get; set; } = "";

    private string _type;

    [JsonPropertyName("type")]
    public string Type {
        get => _type;
        set {
            _type = value;
            // if (RawContent is not null && this is StateEventResponse stateEventResponse) {
            //     if (File.Exists($"unknown_state_events/{Type}/{stateEventResponse.EventId}.json")) return;
            //     var x = GetType.Name;
            // }
        }
    }

    [JsonPropertyName("replaces_state")]
    public string? ReplacesState { get; set; }

    private JsonObject? _rawContent;

    [JsonPropertyName("content")]
    public JsonObject? RawContent {
        get => _rawContent;
        set {
            _rawContent = value;
            // if (Type is not null && this is StateEventResponse stateEventResponse) {
            //     if (File.Exists($"unknown_state_events/{Type}/{stateEventResponse.EventId}.json")) return;
            //     var x = GetType.Name;
            // }
        }
    }

    [JsonIgnore]
    public Type GetType {
        get {
            if (Type == "m.receipt") {
                return typeof(Dictionary<string, JsonObject>);
            }

            var type = KnownStateEventTypes.FirstOrDefault(x =>
                x.GetCustomAttributes<MatrixEventAttribute>()?.Any(y => y.EventName == Type) ?? false);

            //special handling for some types
            // if (type == typeof(RoomEmotesEventData)) {
            //     RawContent["emote"] = RawContent["emote"]?.AsObject() ?? new JsonObject();
            // }
            //
            // if (this is StateEventResponse stateEventResponse) {
            //     if (type == null || type == typeof(object)) {
            //         Console.WriteLine($"Warning: unknown event type '{Type}'!");
            //         Console.WriteLine(RawContent.ToJson());
            //         Directory.CreateDirectory($"unknown_state_events/{Type}");
            //         File.WriteAllText($"unknown_state_events/{Type}/{stateEventResponse.EventId}.json",
            //             RawContent.ToJson());
            //         Console.WriteLine($"Saved to unknown_state_events/{Type}/{stateEventResponse.EventId}.json");
            //     }
            //     else if (RawContent is not null && RawContent.FindExtraJsonObjectFields(type)) {
            //         Directory.CreateDirectory($"unknown_state_events/{Type}");
            //         File.WriteAllText($"unknown_state_events/{Type}/{stateEventResponse.EventId}.json",
            //             RawContent.ToJson());
            //         Console.WriteLine($"Saved to unknown_state_events/{Type}/{stateEventResponse.EventId}.json");
            //     }
            // }

            return type ?? typeof(object);
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
