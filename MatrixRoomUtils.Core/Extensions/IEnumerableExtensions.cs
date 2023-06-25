using System.Reflection;
using System.Text.Json;
using MatrixRoomUtils.Core.Interfaces;
using MatrixRoomUtils.Core.Responses;

namespace MatrixRoomUtils.Core.Extensions;

public static class IEnumerableExtensions {
    public static List<StateEventResponse> DeserializeMatrixTypes(this List<JsonElement> stateEvents) {
        return stateEvents.Select(DeserializeMatrixType).ToList();
    }
    
    public static StateEventResponse DeserializeMatrixType(this JsonElement stateEvent) {
        var type = stateEvent.GetProperty("type").GetString();
        var knownType = StateEvent.KnownStateEventTypes.FirstOrDefault(x => x.GetCustomAttribute<MatrixEventAttribute>()?.EventName == type);
        if (knownType == null) {
            Console.WriteLine($"Warning: unknown event type '{type}'!");
            return new StateEventResponse();
        }
        
        var eventInstance = Activator.CreateInstance(typeof(StateEventResponse).MakeGenericType(knownType))!;
        stateEvent.Deserialize(eventInstance.GetType());
        
        return (StateEventResponse) eventInstance;
    }

    public static void Replace(this List<StateEvent> stateEvents, StateEvent old, StateEvent @new) {
        var index = stateEvents.IndexOf(old);
        if (index == -1) return;
        stateEvents[index] = @new;
    }
}

public class MatrixEventAttribute : Attribute {
    public string EventName { get; set; }
}