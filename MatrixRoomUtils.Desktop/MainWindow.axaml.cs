using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MatrixRoomUtils.Desktop;

public partial class MainWindow : Window {
    private readonly ILogger<MainWindow> _logger;
    private readonly MRUStorageWrapper _storageWrapper;
    private readonly MRUDesktopConfiguration _configuration;

    public MainWindow(ILogger<MainWindow> logger, IServiceScopeFactory scopeFactory) {
        _logger = logger;
        _configuration = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<MRUDesktopConfiguration>();
        _storageWrapper = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<MRUStorageWrapper>();
        _logger.LogInformation("Initialising MainWindow");

        InitializeComponent();

        _logger.LogInformation("Cache location: " + _configuration.CacheStoragePath);
        _logger.LogInformation("Data location: " + _configuration.DataStoragePath);


        for (int i = 0; i < 100; i++) {
            roomList.Children.Add(new RoomListEntry());
        }
    }

    // ReSharper disable once AsyncVoidMethod
    protected override async void OnLoaded(RoutedEventArgs e) {
        _logger.LogInformation("async onloaded override");
        var hs = await _storageWrapper.GetCurrentSessionOrPrompt();
        base.OnLoaded(e);
    }

    // public Command
    // protected void LoadedCommand() {
        // _logger.LogInformation("async command");
    // }
}