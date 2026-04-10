using CommunityToolkit.Mvvm.ComponentModel;
using HarnessHub.Abstract.Services;
using HarnessHub.Abstract.ViewModels;
using HarnessHub.Models.Harness;

namespace HarnessHub.Setting.ViewModels;

/// <summary>
/// 설정 화면의 ViewModel.
/// 하네스 프로바이더, 테마, 컨텍스트 윈도우 크기를 관리한다.
/// </summary>
public partial class SettingViewModel : ObservableRecipient, IContentViewModel
{
    private readonly IAppSettingsService _appSettings;
    private readonly IThemeService _themeService;
    private readonly IProjectContext _projectContext;

    [ObservableProperty]
    private HarnessProvider _activeProvider;

    [ObservableProperty]
    private bool _isDarkTheme;

    [ObservableProperty]
    private int _contextWindowSize;

    [ObservableProperty]
    private string _globalPath = string.Empty;

    public SettingViewModel(
        IAppSettingsService appSettings,
        IThemeService themeService,
        IProjectContext projectContext)
    {
        _appSettings = appSettings;
        _themeService = themeService;
        _projectContext = projectContext;

        _activeProvider = _appSettings.ActiveProvider;
        _isDarkTheme = _themeService.IsDarkTheme;
        _contextWindowSize = _appSettings.ContextWindowSize;
        _globalPath = _projectContext.GlobalPath;
    }

    partial void OnActiveProviderChanged(HarnessProvider value)
    {
        _appSettings.SetProvider(value);
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        _themeService.SetTheme(value);
    }

    partial void OnContextWindowSizeChanged(int value)
    {
        _appSettings.SetContextWindowSize(value);
    }
}
