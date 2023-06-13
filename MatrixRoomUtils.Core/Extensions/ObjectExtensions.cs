using System.Text.Encodings.Web;
using System.Text.Json;

namespace MatrixRoomUtils.Core.Extensions;

public static class ObjectExtensions {
    public static string ToJson(this object obj, bool indent = true, bool ignoreNull = false, bool unsafeContent = false) {
        var jso = new JsonSerializerOptions();
        if (indent) jso.WriteIndented = true;
        if (ignoreNull) jso.IgnoreNullValues = true;
        if (unsafeContent) jso.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        return JsonSerializer.Serialize(obj, jso);
    }
}