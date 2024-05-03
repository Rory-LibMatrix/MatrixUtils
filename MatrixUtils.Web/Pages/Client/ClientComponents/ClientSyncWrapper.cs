using System.Collections.ObjectModel;
using ArcaneLibs;
using LibMatrix;
using LibMatrix.Helpers;
using LibMatrix.Homeservers;
using LibMatrix.Responses;
using MatrixUtils.Abstractions;

namespace MatrixUtils.Web.Pages.Client.ClientComponents;

public class ClientSyncWrapper(AuthenticatedHomeserverGeneric homeserver) : NotifyPropertyChanged {
    private SyncHelper _syncHelper = new SyncHelper(homeserver) {
        MinimumDelay = TimeSpan.FromMilliseconds(2000),
        IsInitialSync = false
    };
    private string _status = "Loading...";

    public ObservableCollection<StateEvent> AccountData { get; set; } = new();
    public ObservableCollection<RoomInfo> Rooms { get; set; } = new();

    public string Status {
        get => _status;
        set => SetField(ref _status, value);
    }

    public async Task Start() {
        Task.Yield();
        var resp = _syncHelper.EnumerateSyncAsync();
        Status = $"[{DateTime.Now:s}] Syncing...";
        await foreach (var response in resp) {
            Task.Yield();
            Status = $"[{DateTime.Now:s}] {response.Rooms?.Join?.Count ?? 0 + response.Rooms?.Invite?.Count ?? 0 + response.Rooms?.Leave?.Count ?? 0} rooms, {response.AccountData?.Events?.Count ?? 0} account data, {response.ToDevice?.Events?.Count ?? 0} to-device, {response.DeviceLists?.Changed?.Count ?? 0} device lists, {response.Presence?.Events?.Count ?? 0} presence updates";
            await HandleSyncResponse(response);
            await Task.Yield();
        }
    }

    private async Task HandleSyncResponse(SyncResponse resp) {
        
    }
}