using CommunityToolkit.Mvvm.DependencyInjection;
using HarnessHub.Abstract.Services;
using HarnessHub.App.Services;
using HarnessHub.Shell.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HarnessHub.App.Boot.DI;

/// <summary>
/// DI 컨테이너를 구성한다.
/// Services는 Singleton, ViewModels는 Transient로 등록한다.
/// </summary>
public static class IocBuilder
{
    public static void Build()
    {
        var services = new ServiceCollection();

        ConfigureServices(services);
        ConfigureViewModels(services);

        Ioc.Default.ConfigureServices(services.BuildServiceProvider());
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IThemeService, ThemeService>();
    }

    private static void ConfigureViewModels(IServiceCollection services)
    {
        services.AddTransient<ShellWindowViewModel>();
    }
}
