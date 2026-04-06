using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarnessHub.Abstract.Services;
using HarnessHub.Abstract.ViewModels;
using HarnessHub.Models.Explorer;
using HarnessHub.Models.Harness;

namespace HarnessHub.Explorer.ViewModels;

/// <summary>
/// 파일 탐색기 화면의 ViewModel.
/// 글로벌 하네스 경로를 기본 표시하고, 프로젝트 폴더 추가 시 함께 표시한다.
/// </summary>
public partial class ExplorerViewModel : ObservableRecipient, IContentViewModel
{
    private readonly IHarnessScanner _scanner;
    private readonly IFileExplorerService _fileExplorerService;

    [ObservableProperty]
    private string _globalPath;

    [ObservableProperty]
    private string? _projectPath;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private FolderNode? _selectedNode;

    public ObservableCollection<FolderNode> RootNodes { get; } = new();
    public ObservableCollection<HarnessFileInfo> HarnessFiles { get; } = new();

    public ExplorerViewModel(IHarnessScanner scanner, IFileExplorerService fileExplorerService)
    {
        _scanner = scanner;
        _fileExplorerService = fileExplorerService;
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

            // 글로벌 하네스 스캔 (항상)
            var globalFiles = await _scanner.ScanAsync(GlobalPath, HarnessScope.Global);
            allFiles.AddRange(globalFiles);

            // 프로젝트 하네스 스캔 (선택 시)
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

            // 트리 구성
            RootNodes.Clear();

            var globalTree = await _fileExplorerService.BuildFolderTreeAsync(GlobalPath, globalFiles);
            globalTree.IsExpanded = true;
            RootNodes.Add(globalTree);

            if (!string.IsNullOrEmpty(ProjectPath))
            {
                var projectTree = await _fileExplorerService.BuildFolderTreeAsync(ProjectPath,
                    allFiles.Where(f => f.Scope == HarnessScope.Project).ToList());
                projectTree.IsExpanded = true;
                RootNodes.Add(projectTree);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
