using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using HarnessHub.Shell.ViewModels;
using HarnessHub.Shell.Views;
using Serilog;

namespace HarnessHub.App.Boot;

/// <summary>
/// WPF Application 클래스.
/// ShellWindow를 생성하고 ShellWindowViewModel을 DataContext에 바인딩한다.
/// </summary>
public partial class HarnessHubApp : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var shellViewModel = Ioc.Default.GetService<ShellWindowViewModel>();
        if (shellViewModel is null)
        {
            throw new InvalidOperationException("ShellWindowViewModel is not registered in DI container.");
        }

        var shellWindow = new ShellWindow
        {
            DataContext = shellViewModel
        };

        MainWindow = shellWindow;
        shellWindow.Show();

        Log.Information("ShellWindow created and shown");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("HarnessHub shutting down");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
