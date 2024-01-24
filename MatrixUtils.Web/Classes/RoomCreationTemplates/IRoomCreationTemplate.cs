using LibMatrix.Responses;

namespace MatrixUtils.Web.Classes.RoomCreationTemplates;

public interface IRoomCreationTemplate {
    public CreateRoomRequest CreateRoomRequest { get; }
    public string Name { get; }
}
