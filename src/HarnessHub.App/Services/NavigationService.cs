using CommunityToolkit.Mvvm.DependencyInjection;
using HarnessHub.Abstract.Services;
using HarnessHub.Abstract.ViewModels;
using HarnessHub.Dashboard.ViewModels;
using HarnessHub.Explorer.ViewModels;

namespace HarnessHub.App.Services;

/// <summary>
/// 네비게이션 인덱스에 따라 콘텐츠 ViewModel을 DI에서 해석한다.
/// </summary>
public sealed class NavigationService : INavigationService
{
    public IContentViewModel? ResolveContent(int index)
    {
        return index switch
        {
            0 => Ioc.Default.GetService<DashboardViewModel>(),
            1 => Ioc.Default.GetService<ExplorerViewModel>(),
            // 2: Editor (Phase 4)
            // 3: Preset (Phase 5)
            _ => null
        };
    }
}
