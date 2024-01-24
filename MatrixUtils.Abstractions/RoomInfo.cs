using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using ArcaneLibs;
using LibMatrix;
using LibMatrix.EventTypes.Spec.State;
using LibMatrix.EventTypes.Spec.State.RoomInfo;
using LibMatrix.RoomTypes;

namespace MatrixUtils.Abstractions;

public class RoomInfo : NotifyPropertyChanged {
    public required GenericRoom Room { get; set; }
    public ObservableCollection<StateEventResponse?> StateEvents { get; } = new();

    public async Task<StateEventResponse?> GetStateEvent(string type, string stateKey = "") {
        var @event = StateEvents.FirstOrDefault(x => x.Type == type && x.StateKey == stateKey);
        if (@event is not null) return @event;
        @event = new StateEventResponse {
            RoomId = Room.RoomId,
            Type = type,
            StateKey = stateKey,
            Sender = null, //TODO implement
            EventId = null
        };
        // if (Room is null) return null;
        try {
            @event.RawContent = await Room.GetStateAsync<JsonObject>(type, stateKey);
        }
        catch (MatrixException e) {
            if (e is { ErrorCode: "M_NOT_FOUND" }) {
                if (type == "m.room.name")
                    @event = new() {
                        Type = type,
                        StateKey = stateKey,
                        TypedContent = new RoomNameEventContent() {
                            Name = await Room.GetNameOrFallbackAsync()
                        },
                        //TODO implement
                        RoomId = null,
                        Sender = null,
                        EventId = null
                    };
                else
                    @event.RawContent = default!;
            }
            else {
                throw;
            }
        }

        StateEvents.Add(@event);
        return @event;
    }

    public string? RoomIcon {
        get => _roomIcon ?? "https://api.dicebear.com/6.x/identicon/svg?seed=" + Room.RoomId;
        set => SetField(ref _roomIcon, value);
    }

    public string? RoomName {
        get => _roomName ?? DefaultRoomName ?? Room.RoomId;
        set => SetField(ref _roomName, value);
    }

    public RoomCreateEventContent? CreationEventContent {
        get => _creationEventContent;
        set => SetField(ref _creationEventContent, value);
    }

    public string? RoomCreator {
        get => _roomCreator;
        set => SetField(ref _roomCreator, value);
    }

    // public string? GetRoomIcon() => (StateEvents.FirstOrDefault(x => x?.Type == RoomAvatarEventContent.EventId)?.TypedContent as RoomAvatarEventContent)?.Url ??
    // "mxc://rory.gay/dgP0YPjJEWaBwzhnbyLLwGGv";

    private string? _roomIcon;
    private string? _roomName;
    private RoomCreateEventContent? _creationEventContent;
    private string? _roomCreator;

    public string? DefaultRoomName { get; set; }

    public RoomInfo() {
        StateEvents.CollectionChanged += (_, args) => {
            if (args.NewItems is { Count: > 0 })
                foreach (StateEventResponse newState in args.NewItems) { // TODO: switch statement benchmark?
                    if (newState.Type == RoomNameEventContent.EventId && newState.TypedContent is RoomNameEventContent roomNameContent)
                        RoomName = roomNameContent.Name;
                    else if (newState is { Type: RoomAvatarEventContent.EventId, TypedContent: RoomAvatarEventContent roomAvatarContent })
                        RoomIcon = roomAvatarContent.Url;
                    else if (newState is { Type: RoomCreateEventContent.EventId, TypedContent: RoomCreateEventContent roomCreateContent }) {
                        CreationEventContent = roomCreateContent;
                        RoomCreator = newState.Sender;
                    }
                }
        };
    }
}
