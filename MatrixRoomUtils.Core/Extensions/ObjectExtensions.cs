using System.Text.Json;

namespace MatrixRoomUtils.Core.Extensions;

public static class ObjectExtensions
{
    public static string ToJson(this object obj, bool indent = true, bool ignoreNull = false)
    {
        var jso = new JsonSerializerOptions();
        if(indent) jso.WriteIndented = true;
        if(ignoreNull) jso.IgnoreNullValues = true;
        return JsonSerializer.Serialize(obj, jso);
    }
}