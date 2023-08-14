using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MatrixRoomUtils.Desktop.Components;

public partial class NavigationStack : UserControl {
    public NavigationStack() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        AvaloniaXamlLoader.Load(this);
        buildView();
    }

    private void buildView() {
        navPanel.Children.Clear();
        foreach (var item in _stack) {
            Button btn = new() {
                Content = item.Name
            };
            btn.Click += (_, _) => {
                PopTo(_stack.IndexOf(item));
                buildView();
            };
            navPanel.Children.Add(btn);
        }
        content = Current?.View ?? new UserControl();
    }


    public class NavigationStackItem {
        public string Name { get; set; }
        public string Description { get; set; } = "";
        public UserControl View { get; set; }
    }

    private List<NavigationStackItem> _stack = new();

    public NavigationStackItem? Current => _stack.LastOrDefault();

    public void Push(string name, UserControl view) {
        _stack.Add(new NavigationStackItem {
            Name = name,
            View = view
        });
    }

    public void Pop() {
        _stack.RemoveAt(_stack.Count - 1);
    }

    public void PopTo(int index) {
        _stack.RemoveRange(index, _stack.Count - index);
    }
}
