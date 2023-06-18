using System.Runtime.CompilerServices;

namespace MatrixRoomUtils.Core.Attributes; 

[AttributeUsage(AttributeTargets.All)]
public class TraceAttribute : Attribute {
    public TraceAttribute([CallerMemberName] string callerName = "") {
        Console.WriteLine($"{callerName} called!");
    }
}