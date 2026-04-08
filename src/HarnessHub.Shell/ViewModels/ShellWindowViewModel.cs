using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HarnessHub.Abstract.Services;
using HarnessHub.Abstract.ViewModels;
using HarnessHub.Models.Messages;

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

    /// <summary>
    /// OpenFileMessage로 전달된 파일 경로. 에디터로 전환 후 로드에 사용된다.
    /// </summary>
    private string? _pendingFilePath;

    public ShellWindowViewModel(IThemeService themeService, INavigationService navigationService)
    {
        _themeService = themeService;
        _navigationService = navigationService;
        _isDarkTheme = _themeService.IsDarkTheme;

        IsActive = true;

        NavigateTo(0);
    }

    /// <inheritdoc />
    protected override void OnActivated()
    {
        Messenger.Register<OpenFileMessage>(this, (r, m) =>
        {
            _pendingFilePath = m.FilePath;
            SelectedNavigationIndex = 2;
        });
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
    /// OpenFileMessage로 파일 경로가 전달된 경우 에디터에 파일을 로드한다.
    /// </summary>
    private void NavigateTo(int index)
    {
        CurrentContent = _navigationService.ResolveContent(index);

        if (_pendingFilePath is not null && CurrentContent is IFileEditor editor)
        {
            var filePath = _pendingFilePath;
            _pendingFilePath = null;
            _ = editor.LoadFileAsync(filePath);
        }
    }
}
