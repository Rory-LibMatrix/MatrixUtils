using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using LibMatrix;
using LibMatrix.Helpers;
using LibMatrix.Services;
using LibMatrix.StateEventTypes.Spec;
using MatrixRoomUtils.Web.Classes;
using Microsoft.Extensions.DependencyInjection;

namespace MatrixRoomUtils.Desktop;

public partial class RoomListEntry : UserControl {
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly RoomInfo _roomInfo;

    public RoomListEntry(IServiceScopeFactory serviceScopeFactory, RoomInfo roomInfo) {
        _serviceScopeFactory = serviceScopeFactory;
        _roomInfo = roomInfo;
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        RoomName.Content = _roomInfo.Room.RoomId;
        Task.WhenAll(GetRoomName(), GetRoomIcon());
    }

    private async Task GetRoomName() {
        try {
            var nameEvent = await _roomInfo.GetStateEvent("m.room.name");
            if (nameEvent is not null && nameEvent.TypedContent is RoomNameEventData nameData)
                RoomName.Content = nameData.Name;
        }
        catch (MatrixException e) {
            if (e.ErrorCode != "M_NOT_FOUND")
                throw;
        }
    }

    private async Task GetRoomIcon() {
        try {
            var avatarEvent = await _roomInfo.GetStateEvent("m.room.avatar");
            if (avatarEvent is not null && avatarEvent.TypedContent is RoomAvatarEventData avatarData) {
                var mxcUrl = avatarData.Url;
                await using var svc = _serviceScopeFactory.CreateAsyncScope();
                var hs = await svc.ServiceProvider.GetService<MRUStorageWrapper>().GetCurrentSessionOrPrompt();
                var storage = svc.ServiceProvider.GetService<TieredStorageService>().CacheStorageProvider;
                var resolvedUrl = MediaResolver.ResolveMediaUri(hs.FullHomeServerDomain, mxcUrl);
                var storageKey = $"media/{mxcUrl.Replace("mxc://", "").Replace("/", ".")}";
                try {
                    if (!await storage.ObjectExistsAsync(storageKey))
                        await storage.SaveStreamAsync(storageKey, await hs._httpClient.GetStreamAsync(resolvedUrl));

                    RoomIcon.Source = new Bitmap(await storage.LoadStreamAsync(storageKey) ?? throw new NullReferenceException());
                }
                catch (IOException) { }
                catch (MatrixException e) {
                    if (e.ErrorCode != "M_UNKNOWN")
                        throw;
                }
            }
        }
        catch (MatrixException e) {
            if (e.ErrorCode != "M_NOT_FOUND")
                throw;
        }
    }
}
