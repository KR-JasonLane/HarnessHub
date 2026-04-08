using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HarnessHub.Util.Behaviors;

/// <summary>
/// TreeView 항목 더블클릭 시 Command를 실행하는 Attached Behavior.
/// CommandParameter로 더블클릭된 항목의 DataContext를 전달한다.
/// </summary>
public static class TreeViewDoubleClickBehavior
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(TreeViewDoubleClickBehavior),
            new PropertyMetadata(null, OnCommandChanged));

    public static ICommand? GetCommand(DependencyObject obj) => (ICommand?)obj.GetValue(CommandProperty);
    public static void SetCommand(DependencyObject obj, ICommand? value) => obj.SetValue(CommandProperty, value);

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TreeView treeView)
            return;

        if (e.OldValue is not null)
        {
            treeView.MouseDoubleClick -= OnMouseDoubleClick;
        }

        if (e.NewValue is not null)
        {
            treeView.MouseDoubleClick += OnMouseDoubleClick;
        }
    }

    private static void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TreeView treeView)
            return;

        var command = GetCommand(treeView);
        if (command is null)
            return;

        var selectedItem = treeView.SelectedItem;
        if (selectedItem is not null && command.CanExecute(selectedItem))
        {
            command.Execute(selectedItem);
        }
    }
}
