using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core.Extensions;

public static class JsonElementExtensions {
    public static void FindExtraJsonFields([DisallowNull] this JsonElement? res, Type t) {
        var props = t.GetProperties();
        var unknownPropertyFound = false;
        foreach (var field in res.Value.EnumerateObject()) {
            if (props.Any(x => x.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name == field.Name)) continue;
            Console.WriteLine($"[!!] Unknown property {field.Name} in {t.Name}!");
            unknownPropertyFound = true;
        }

        if (unknownPropertyFound) Console.WriteLine(res.Value.ToJson());
    }
}