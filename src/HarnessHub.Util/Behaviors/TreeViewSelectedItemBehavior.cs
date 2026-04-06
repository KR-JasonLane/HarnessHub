using System.Windows;
using System.Windows.Controls;

namespace HarnessHub.Util.Behaviors;

/// <summary>
/// TreeView의 읽기 전용 SelectedItem을 바인딩 가능하게 하는 Attached Behavior.
/// View가 ViewModel을 직접 참조하지 않고도 선택 항목을 전달할 수 있다.
/// </summary>
public static class TreeViewSelectedItemBehavior
{
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItem",
            typeof(object),
            typeof(TreeViewSelectedItemBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(TreeViewSelectedItemBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static object GetSelectedItem(DependencyObject obj) =>
        obj.GetValue(SelectedItemProperty);

    public static void SetSelectedItem(DependencyObject obj, object value) =>
        obj.SetValue(SelectedItemProperty, value);

    public static bool GetIsEnabled(DependencyObject obj) =>
        (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) =>
        obj.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TreeView treeView)
            return;

        if ((bool)e.NewValue)
        {
            treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
        }
        else
        {
            treeView.SelectedItemChanged -= TreeView_SelectedItemChanged;
        }
    }

    private static void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (sender is TreeView treeView)
        {
            SetSelectedItem(treeView, e.NewValue);
        }
    }
}
