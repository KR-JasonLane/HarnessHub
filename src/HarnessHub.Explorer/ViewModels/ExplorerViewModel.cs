using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HarnessHub.Abstract.Services;
using HarnessHub.Abstract.ViewModels;
using HarnessHub.Models.Explorer;
using HarnessHub.Models.Harness;
using HarnessHub.Models.Messages;

namespace HarnessHub.Explorer.ViewModels;

/// <summary>
/// 파일 탐색기 화면의 ViewModel.
/// 글로벌 하네스 경로를 기본 표시하고, 프로젝트 폴더 추가 시 함께 표시한다.
/// </summary>
public partial class ExplorerViewModel : ObservableRecipient, IContentViewModel
{
    private readonly IHarnessScanner _scanner;
    private readonly IFileExplorerService _fileExplorerService;
    private readonly IProjectContext _projectContext;
    private readonly IFileDialogService _fileDialog;

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

    public ExplorerViewModel(
        IHarnessScanner scanner,
        IFileExplorerService fileExplorerService,
        IProjectContext projectContext,
        IFileDialogService fileDialog)
    {
        _scanner = scanner;
        _fileExplorerService = fileExplorerService;
        _projectContext = projectContext;
        _fileDialog = fileDialog;
        _globalPath = _projectContext.GlobalPath;
        _projectPath = _projectContext.ProjectPath;

        IsActive = true;

        _ = LoadAsync();
    }

    /// <inheritdoc />
    protected override void OnActivated()
    {
        _projectContext.ProjectPathChanged += OnProjectPathChangedEvent;
    }

    /// <inheritdoc />
    protected override void OnDeactivated()
    {
        _projectContext.ProjectPathChanged -= OnProjectPathChangedEvent;
    }

    private void OnProjectPathChangedEvent(string path)
    {
        ProjectPath = path;
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

    /// <summary>
    /// 하네스 파일을 에디터에서 연다. DataGrid 행 또는 트리뷰 파일 더블클릭 시 호출.
    /// </summary>
    [RelayCommand]
    private void OpenFile(object? parameter)
    {
        string? filePath = parameter switch
        {
            HarnessFileInfo file => file.FilePath,
            FolderNode { IsDirectory: false, IsHarnessFile: true } node => node.FullPath,
            _ => null
        };

        if (filePath is not null)
        {
            WeakReferenceMessenger.Default.Send(new OpenFileMessage(filePath));
        }
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
