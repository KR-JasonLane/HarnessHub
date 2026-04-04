using HarnessHub.App.Boot.DI;
using HarnessHub.App.Logging;

namespace HarnessHub.App.Boot;

/// <summary>
/// 애플리케이션 진입점.
/// DI 컨테이너와 로깅을 초기화한 후 WPF Application을 실행한다.
/// </summary>
public static class BootStrapper
{
    [STAThread]
    public static void Main()
    {
        LogBuilder.Build();
        IocBuilder.Build();

        var app = new HarnessHubApp();
        app.InitializeComponent();
        app.Run();
    }
}
