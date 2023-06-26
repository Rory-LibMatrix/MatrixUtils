using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core.Extensions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MatrixEventAttribute : Attribute {
    public string EventName { get; set; }
    public bool Legacy { get; set; }
}