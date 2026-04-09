using System.Collections.Concurrent;
using CommunityToolkit.Mvvm.DependencyInjection;
using HarnessHub.Abstract.Services;
using HarnessHub.Abstract.ViewModels;
using HarnessHub.Dashboard.ViewModels;
using HarnessHub.Editor.ViewModels;
using HarnessHub.Explorer.ViewModels;
using HarnessHub.Preset.ViewModels;

namespace HarnessHub.App.Services;

/// <summary>
/// 네비게이션 인덱스에 따라 콘텐츠 ViewModel을 DI에서 해석한다.
/// 한번 생성된 ViewModel은 캐싱하여 탭 전환 시 상태를 유지한다.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private readonly ConcurrentDictionary<int, IContentViewModel> _cache = new();

    public IContentViewModel? ResolveContent(int index)
    {
        if (_cache.TryGetValue(index, out var cached))
            return cached;

        var viewModel = CreateContent(index);
        if (viewModel is null)
            return null;

        _cache.TryAdd(index, viewModel);
        return viewModel;
    }

    private static IContentViewModel? CreateContent(int index)
    {
        IContentViewModel? viewModel = index switch
        {
            0 => Ioc.Default.GetService<DashboardViewModel>(),
            1 => Ioc.Default.GetService<ExplorerViewModel>(),
            2 => Ioc.Default.GetService<MarkdownEditorViewModel>(),
            3 => Ioc.Default.GetService<PresetViewModel>(),
            _ => null
        };

        if (viewModel is null && index <= 3)
            throw new InvalidOperationException($"DI에 인덱스 {index}에 해당하는 ViewModel이 등록되지 않았습니다.");

        return viewModel;
    }
}
