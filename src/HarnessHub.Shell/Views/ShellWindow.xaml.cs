using System.Windows;

namespace HarnessHub.Shell.Views;

/// <summary>
/// ShellWindow 코드비하인드.
/// MVVM 패턴에 따라 InitializeComponent()만 호출한다.
/// </summary>
public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
    }
}
