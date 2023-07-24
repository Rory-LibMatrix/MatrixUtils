using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;

namespace MatrixRoomUtils.Desktop;

public partial class RoomListEntry : UserControl {
    public RoomListEntry() {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        RoomName.Content = "asdf";
        RoomIcon.Source = new Bitmap("/home/root@Rory/giphy.gif");
    }
}
