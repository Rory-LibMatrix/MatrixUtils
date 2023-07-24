using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;
using MatrixRoomUtils.Core.StateEventTypes;
using MatrixRoomUtils.Core.StateEventTypes.Spec;

namespace MatrixRoomUtils.Core.Responses;

public class CreateRoomRequest {
    [JsonIgnore] public CreationContentBaseType _creationContentBaseType;

    public CreateRoomRequest() => _creationContentBaseType = new CreationContentBaseType(this);

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("room_alias_name")]
    public string RoomAliasName { get; set; } = null!;

    //we dont want to use this, we want more control
    // [JsonPropertyName("preset")]
    // public string Preset { get; set; } = null!;

    [JsonPropertyName("initial_state")]
    public List<StateEvent> InitialState { get; set; } = null!;

    [JsonPropertyName("visibility")]
    public string Visibility { get; set; } = null!;

    [JsonPropertyName("power_level_content_override")]
    public RoomPowerLevelEventData PowerLevelContentOverride { get; set; } = null!;

    [JsonPropertyName("creation_content")]
    public JsonObject CreationContent { get; set; } = new();

    /// <summary>
    ///     For use only when you can't use the CreationContent property
    /// </summary>

    public StateEvent this[string event_type, string event_key = ""] {
        get {
            var stateEvent = InitialState.FirstOrDefault(x => x.Type == event_type && x.StateKey == event_key);
            if (stateEvent == null) {
                InitialState.Add(stateEvent = new StateEvent {
                    Type = event_type,
                    StateKey = event_key,
                    TypedContent = Activator.CreateInstance(
                        StateEvent.KnownStateEventTypes.FirstOrDefault(x =>
                            x.GetCustomAttributes<MatrixEventAttribute>()?
                                .Any(y => y.EventName == event_type) ?? false) ?? typeof(object)
                        )
                });
            }
            return stateEvent;
        }
        set {
            var stateEvent = InitialState.FirstOrDefault(x => x.Type == event_type && x.StateKey == event_key);
            if (stateEvent == null)
                InitialState.Add(value);
            else
                InitialState[InitialState.IndexOf(stateEvent)] = value;
        }
    }

    public Dictionary<string, string> Validate() {
        Dictionary<string, string> errors = new();
        if (!Regex.IsMatch(RoomAliasName, @"[a-zA-Z0-9_\-]+$"))
            errors.Add("room_alias_name",
                "Room alias name must only contain letters, numbers, underscores, and hyphens.");

        return errors;
    }
}
