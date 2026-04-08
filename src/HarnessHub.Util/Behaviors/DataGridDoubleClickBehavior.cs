using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace HarnessHub.Util.Behaviors;

/// <summary>
/// DataGrid 행 더블클릭 시 Command를 실행하는 Attached Behavior.
/// CommandParameter로 더블클릭된 행의 DataContext를 전달한다.
/// </summary>
public static class DataGridDoubleClickBehavior
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(DataGridDoubleClickBehavior),
            new PropertyMetadata(null, OnCommandChanged));

    public static ICommand? GetCommand(DependencyObject obj) => (ICommand?)obj.GetValue(CommandProperty);
    public static void SetCommand(DependencyObject obj, ICommand? value) => obj.SetValue(CommandProperty, value);

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid)
            return;

        if (e.OldValue is not null)
        {
            dataGrid.MouseDoubleClick -= OnMouseDoubleClick;
        }

        if (e.NewValue is not null)
        {
            dataGrid.MouseDoubleClick += OnMouseDoubleClick;
        }
    }

    private static void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid)
            return;

        var command = GetCommand(dataGrid);
        if (command is null)
            return;

        // 헤더 더블클릭은 무시
        if (e.OriginalSource is DependencyObject source)
        {
            var header = FindParent<DataGridColumnHeader>(source);
            if (header is not null)
                return;
        }

        var selectedItem = dataGrid.SelectedItem;
        if (selectedItem is not null && command.CanExecute(selectedItem))
        {
            command.Execute(selectedItem);
        }
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
        while (parent is not null)
        {
            if (parent is T found)
                return found;
            parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
        }

        return null;
    }
}
