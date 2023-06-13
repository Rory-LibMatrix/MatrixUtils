using MatrixRoomUtils.Core.Responses;

namespace MatrixRoomUtils.Web.Classes.RoomCreationTemplates;

public interface IRoomCreationTemplate {
    public CreateRoomRequest CreateRoomRequest { get; }
    public string Name { get; }
}