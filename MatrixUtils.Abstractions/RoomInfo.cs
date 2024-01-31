using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using ArcaneLibs;
using LibMatrix;
using LibMatrix.EventTypes.Spec.State;
using LibMatrix.EventTypes.Spec.State.RoomInfo;
using LibMatrix.Homeservers;
using LibMatrix.RoomTypes;

namespace MatrixUtils.Abstractions;

public class RoomInfo : NotifyPropertyChanged {
    public required GenericRoom Room { get; set; }
    public ObservableCollection<StateEventResponse?> StateEvents { get; } = new();

    private static ConcurrentBag<AuthenticatedHomeserverGeneric> homeserversWithoutEventFormatSupport = new();
    
    public async Task<StateEventResponse?> GetStateEvent(string type, string stateKey = "") {
        if (homeserversWithoutEventFormatSupport.Contains(Room.Homeserver)) return await GetStateEventForged(type, stateKey);
        var @event = StateEvents.FirstOrDefault(x => x?.Type == type && x.StateKey == stateKey);
        if (@event is not null) return @event;
        
        try {
            @event = await Room.GetStateEventOrNullAsync(type, stateKey);
            StateEvents.Add(@event);
        }
        catch (Exception e) {
            if (e is InvalidDataException) {
                homeserversWithoutEventFormatSupport.Add(Room.Homeserver);
                return await GetStateEventForged(type, stateKey);
            }
            Console.Error.WriteLine(e);
            await Task.Delay(1000);
            return await GetStateEvent(type, stateKey);
        }

        return @event;
    }

    private async Task<StateEventResponse?> GetStateEventForged(string type, string stateKey = "") {
        var @event = new StateEventResponse {
            RoomId = Room.RoomId,
            Type = type,
            StateKey = stateKey,
            Sender = null, //TODO implement
            EventId = null
        };
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
        catch (Exception e) {
            await Task.Delay(1000);
            return await GetStateEvent(type, stateKey);
        }

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
    private string? _overrideRoomType;
    private string? _defaultRoomName;
    private RoomMemberEventContent? _ownMembership;

    public string? DefaultRoomName {
        get => _defaultRoomName;
        set {
            if (SetField(ref _defaultRoomName, value)) OnPropertyChanged(nameof(RoomName));
        }
    }

    public string? OverrideRoomType {
        get => _overrideRoomType;
        set {
            if (SetField(ref _overrideRoomType, value)) OnPropertyChanged(nameof(RoomType));
        }
    }

    public string? RoomType => OverrideRoomType ?? CreationEventContent?.Type;

    public RoomMemberEventContent? OwnMembership {
        get => _ownMembership;
        set => SetField(ref _ownMembership, value);
    }

    public RoomInfo() {
        StateEvents.CollectionChanged += (_, args) => {
            if (args.NewItems is { Count: > 0 })
                foreach (StateEventResponse? newState in args.NewItems) { // TODO: switch statement benchmark?
                    if(newState is null) continue;
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
