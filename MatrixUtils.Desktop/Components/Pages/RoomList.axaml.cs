using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MatrixUtils.Abstractions;

namespace MatrixUtils.Desktop.Components.Pages;

public partial class RoomList : UserControl {
    private ObservableCollection<RoomInfo> Rooms { get; set; } = new();

    public RoomList() {
        InitializeComponent();
    }
}