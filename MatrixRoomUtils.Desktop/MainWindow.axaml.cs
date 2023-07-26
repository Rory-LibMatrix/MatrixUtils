using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using MatrixRoomUtils.Web.Classes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MatrixRoomUtils.Desktop;

public partial class MainWindow : Window {
    private readonly ILogger<MainWindow> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MRUStorageWrapper _storageWrapper;
    private readonly MRUDesktopConfiguration _configuration;
    public static MainWindow Instance { get; private set; } = null!;

    public MainWindow(ILogger<MainWindow> logger, IServiceScopeFactory scopeFactory, SentryService _) {
        Instance = this;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _configuration = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<MRUDesktopConfiguration>();
        _storageWrapper = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<MRUStorageWrapper>();

        _logger.LogInformation("Initialising MainWindow");

        InitializeComponent();

        _logger.LogInformation("Cache location: " + _configuration.CacheStoragePath);
        _logger.LogInformation("Data location: " + _configuration.DataStoragePath);


        // for (int i = 0; i < 100; i++) {
            // roomList.Children.Add(new RoomListEntry());
        // }
    }

    // ReSharper disable once AsyncVoidMethod
    protected override async void OnLoaded(RoutedEventArgs e) {
        _logger.LogInformation("async onloaded override");
        var hs = await _storageWrapper.GetCurrentSessionOrPrompt();
        var rooms = await hs.GetJoinedRooms();
        foreach (var room in rooms) {
            roomList.Children.Add(new RoomListEntry(_scopeFactory, new RoomInfo(room)));
        }
        base.OnLoaded(e);
    }

    // public Command
    // protected void LoadedCommand() {
        // _logger.LogInformation("async command");
    // }
}
