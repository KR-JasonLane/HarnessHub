using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarnessHub.Abstract.Services;
using HarnessHub.Abstract.ViewModels;

namespace HarnessHub.Shell.ViewModels;

/// <summary>
/// 메인 윈도우 ViewModel.
/// NavigationRail 선택에 따라 CurrentContent를 전환한다.
/// </summary>
public partial class ShellWindowViewModel : ObservableRecipient
{
    private readonly IThemeService _themeService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private IContentViewModel? _currentContent;

    [ObservableProperty]
    private int _selectedNavigationIndex;

    [ObservableProperty]
    private bool _isDarkTheme;

    public ShellWindowViewModel(IThemeService themeService, INavigationService navigationService)
    {
        _themeService = themeService;
        _navigationService = navigationService;
        _isDarkTheme = _themeService.IsDarkTheme;

        NavigateTo(0);
    }

    partial void OnSelectedNavigationIndexChanged(int value)
    {
        NavigateTo(value);
    }

    partial void OnIsDarkThemeChanged(bool value)
    {
        _themeService.SetTheme(value);
    }

    /// <summary>
    /// Light/Dark 테마를 토글한다.
    /// </summary>
    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
    }

    /// <summary>
    /// NavigationRail 인덱스에 따라 콘텐츠 ViewModel을 전환한다.
    /// </summary>
    private void NavigateTo(int index)
    {
        CurrentContent = _navigationService.ResolveContent(index);
    }
}
