using System.IO;
using Serilog;

namespace HarnessHub.App.Logging;

/// <summary>
/// Serilog 로거를 초기화한다.
/// 로그 파일은 %AppData%/HarnessHub/logs/ 에 저장된다.
/// </summary>
public static class LogBuilder
{
    public static void Build()
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HarnessHub",
            "logs",
            "harnesshub-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("HarnessHub started");
    }
}
