using System.Collections.Concurrent;
using HarnessHub.Abstract.Services;
using HarnessHub.Abstract.ViewModels;
using HarnessHub.Dashboard.ViewModels;
using HarnessHub.Editor.ViewModels;
using HarnessHub.Explorer.ViewModels;
using HarnessHub.Preset.ViewModels;
using HarnessHub.Setting.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HarnessHub.App.Services;

/// <summary>
/// 네비게이션 인덱스에 따라 콘텐츠 ViewModel을 DI에서 해석한다.
/// 한번 생성된 ViewModel은 캐싱하여 탭 전환 시 상태를 유지한다.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<int, IContentViewModel> _cache = new();

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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

    private IContentViewModel? CreateContent(int index)
    {
        IContentViewModel? viewModel = index switch
        {
            0 => _serviceProvider.GetService<DashboardViewModel>(),
            1 => _serviceProvider.GetService<ExplorerViewModel>(),
            2 => _serviceProvider.GetService<MarkdownEditorViewModel>(),
            3 => _serviceProvider.GetService<PresetViewModel>(),
            4 => _serviceProvider.GetService<SettingViewModel>(),
            _ => null
        };

        if (viewModel is null && index <= 4)
            throw new InvalidOperationException($"DI에 인덱스 {index}에 해당하는 ViewModel이 등록되지 않았습니다.");

        return viewModel;
    }
}
