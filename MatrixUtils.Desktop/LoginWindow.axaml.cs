using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace MatrixUtils.Desktop;

public partial class LoginWindow : Window {
    private readonly RMUStorageWrapper _storage;

    public LoginWindow(RMUStorageWrapper storage) {
        _storage = storage;
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
    }

    public string Username { get; set; }
    public string Password { get; set; }
    // ReSharper disable once AsyncVoidMethod
    private async void Login(object? sender, RoutedEventArgs e) {
        var res = await _storage.Login(Username.Split(':')[1], Username.Split(':')[0][1..], Password);
        if (res is not null) {
            await _storage.AddToken(res);
            Close();
        }
        else {
            Password = "";
        }
    }
}
