using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarnessHub.Abstract.Services;
using HarnessHub.Abstract.ViewModels;
using HarnessHub.Models.Harness;

namespace HarnessHub.Dashboard.ViewModels;

/// <summary>
/// 대시보드 화면의 ViewModel. 하네스 구성 현황을 요약하여 표시한다.
/// </summary>
public partial class DashboardViewModel : ObservableRecipient, IContentViewModel
{
    private readonly IHarnessScanner _scanner;
    private readonly ITokenCounterService _tokenCounter;

    [ObservableProperty]
    private string _globalPath;

    [ObservableProperty]
    private string? _projectPath;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalTokens;

    [ObservableProperty]
    private int _contextWindowSize = 200_000;

    [ObservableProperty]
    private double _usagePercentage;

    public ObservableCollection<LeverStatus> LeverStatuses { get; } = new();
    public ObservableCollection<HarnessFileInfo> HarnessFiles { get; } = new();

    public DashboardViewModel(IHarnessScanner scanner, ITokenCounterService tokenCounter)
    {
        _scanner = scanner;
        _tokenCounter = tokenCounter;
        _globalPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".claude");

        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task OpenFolderAsync()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "프로젝트 폴더 선택"
        };

        if (dialog.ShowDialog() == true)
        {
            ProjectPath = dialog.FolderName;
            await LoadAsync();
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync();
    }

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
            UsagePercentage = ContextWindowSize > 0
                ? (double)TotalTokens / ContextWindowSize * 100
                : 0;

            BuildLeverStatuses(allFiles);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildLeverStatuses(List<HarnessFileInfo> files)
    {
        LeverStatuses.Clear();

        foreach (var lever in Enum.GetValues<HarnessLever>())
        {
            var leverFiles = files.Where(f => f.Lever == lever).ToList();
            LeverStatuses.Add(new LeverStatus
            {
                Lever = lever,
                IsActive = leverFiles.Count > 0,
                FileCount = leverFiles.Count,
                Description = GetLeverDescription(lever)
            });
        }
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
