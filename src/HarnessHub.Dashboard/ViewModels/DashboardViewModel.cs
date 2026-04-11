using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HarnessHub.Abstract.Services;
using HarnessHub.Abstract.ViewModels;
using HarnessHub.Models.Harness;
using HarnessHub.Models.Messages;
using Serilog;

namespace HarnessHub.Dashboard.ViewModels;

/// <summary>
/// 대시보드 화면의 ViewModel. 하네스 구성 현황을 요약하여 표시한다.
/// </summary>
public partial class DashboardViewModel : ObservableRecipient, IContentViewModel
{
    private readonly IHarnessScanner _scanner;
    private readonly ITokenCounterService _tokenCounter;
    private readonly IProjectContext _projectContext;
    private readonly IAppSettingsService _appSettings;
    private readonly IFileDialogService _fileDialog;

    [ObservableProperty]
    private string _globalPath;

    [ObservableProperty]
    private string? _projectPath;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalTokens;

    [ObservableProperty]
    private int _contextWindowSize;

    [ObservableProperty]
    private double _usagePercentage;

    public ObservableCollection<LeverStatus> LeverStatuses { get; } = new();
    public ObservableCollection<HarnessFileInfo> HarnessFiles { get; } = new();

    public DashboardViewModel(
        IHarnessScanner scanner,
        ITokenCounterService tokenCounter,
        IProjectContext projectContext,
        IAppSettingsService appSettings,
        IFileDialogService fileDialog)
    {
        _scanner = scanner;
        _tokenCounter = tokenCounter;
        _projectContext = projectContext;
        _appSettings = appSettings;
        _fileDialog = fileDialog;
        _globalPath = _projectContext.GlobalPath;
        _projectPath = _projectContext.ProjectPath;
        _contextWindowSize = _appSettings.ContextWindowSize;

        IsActive = true;

        _ = LoadAsync();
    }

    /// <inheritdoc />
    protected override void OnActivated()
    {
        _projectContext.ProjectPathChanged += OnProjectPathChangedEvent;
        _appSettings.ProviderChanged += OnProviderChangedEvent;

        Messenger.Register<PresetAppliedMessage>(this, (r, m) =>
        {
            _ = LoadAsync();
        });
    }

    /// <inheritdoc />
    protected override void OnDeactivated()
    {
        _projectContext.ProjectPathChanged -= OnProjectPathChangedEvent;
        _appSettings.ProviderChanged -= OnProviderChangedEvent;
    }

    private void OnProjectPathChangedEvent(string path)
    {
        ProjectPath = path;
        _ = LoadAsync();
    }

    private void OnProviderChangedEvent(HarnessProvider provider)
    {
        _ = LoadAsync();
    }

    [RelayCommand]
    private void OpenFolder()
    {
        var folderPath = _fileDialog.ShowOpenFolderDialog("프로젝트 폴더 선택");
        if (folderPath is not null)
        {
            _projectContext.SetProjectPath(folderPath);
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

    private static readonly HarnessLever[] AllLevers = Enum.GetValues<HarnessLever>();

    private async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            var allFiles = new List<HarnessFileInfo>();

            var globalFiles = await _scanner.ScanAsync(GlobalPath, HarnessScope.Global);
            allFiles.AddRange(globalFiles);

            if (!string.IsNullOrEmpty(ProjectPath))
            {
                var projectFiles = await _scanner.ScanAsync(ProjectPath, HarnessScope.Project);
                allFiles.AddRange(projectFiles);
            }

            HarnessFiles.Clear();
            foreach (var file in allFiles)
            {
                HarnessFiles.Add(file);
            }

            TotalTokens = allFiles.Sum(f => f.TokenCount);
            ContextWindowSize = _appSettings.ContextWindowSize;
            UsagePercentage = ContextWindowSize > 0
                ? (double)TotalTokens / ContextWindowSize * 100
                : 0;

            BuildLeverStatuses(allFiles);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "대시보드 로드 실패");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildLeverStatuses(List<HarnessFileInfo> files)
    {
        var byLever = files.GroupBy(f => f.Lever)
                           .ToDictionary(g => g.Key, g => g.Count());

        LeverStatuses.Clear();
        foreach (var lever in AllLevers)
        {
            byLever.TryGetValue(lever, out var count);
            LeverStatuses.Add(new LeverStatus
            {
                Lever = lever,
                IsActive = count > 0,
                FileCount = count,
                Description = GetLeverDescription(lever)
            });
        }
    }

    /// <summary>
    /// 하네스 파일을 에디터에서 연다.
    /// </summary>
    [RelayCommand]
    private void OpenFile(HarnessFileInfo? file)
    {
        if (file is null)
            return;

        WeakReferenceMessenger.Default.Send(new OpenFileMessage(file.FilePath));
    }

    private static string GetLeverDescription(HarnessLever lever) => lever switch
    {
        HarnessLever.SystemPrompt => "시스템 프롬프트",
        HarnessLever.Skill => "스킬",
        HarnessLever.McpServer => "MCP 서버",
        HarnessLever.SubAgent => "서브에이전트",
        HarnessLever.Hook => "Hooks",
        _ => lever.ToString()
    };
}
