using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MatrixRoomUtils.Abstractions;

namespace MatrixRoomUtils.Desktop.Components.Pages;

public partial class RoomList : UserControl {
    private ObservableCollection<RoomInfo> Rooms { get; set; } = new();

    public RoomList() {
        InitializeComponent();
    }
}