using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HarnessHub.Abstract.Services;
using HarnessHub.Abstract.ViewModels;
using HarnessHub.Models.Harness;
using HarnessHub.Models.Messages;

namespace HarnessHub.Editor.ViewModels;

/// <summary>
/// 마크다운 에디터 화면의 ViewModel.
/// 파일 미선택 시 홈 화면(파일 목록 + 생성 가능 목록)을 표시하고,
/// 파일 선택 시 WebView2 에디터로 전환한다.
/// </summary>
public partial class MarkdownEditorViewModel : ObservableRecipient, IContentViewModel, IFileEditor
{
    private readonly IThemeService _themeService;
    private readonly ITokenCounterService _tokenCounter;
    private readonly IProjectContext _projectContext;
    private readonly IHarnessScanner _scanner;

    private string? _originalContent;
    private bool _isNewFile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFileOpen))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(RevertCommand))]
    [NotifyCanExecuteChangedFor(nameof(CloseFileCommand))]
    private string? _filePath;

    [ObservableProperty]
    private string _fileName = "에디터";

    [ObservableProperty]
    private string? _fileType;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(RevertCommand))]
    private bool _isModified;

    [ObservableProperty]
    private int _tokenCount;

    [ObservableProperty]
    private int _lineCount;

    /// <summary>
    /// WebView2에 로드할 마크다운 콘텐츠. Behavior의 MarkdownToLoad에 바인딩된다.
    /// </summary>
    [ObservableProperty]
    private string? _markdownToLoad;

    /// <summary>
    /// WebView2에서 편집된 마크다운 콘텐츠. Behavior의 EditedMarkdown에 바인딩된다.
    /// </summary>
    [ObservableProperty]
    private string? _editedMarkdown;

    /// <summary>
    /// 저장 요청 트리거. Behavior의 RequestSave에 바인딩된다.
    /// </summary>
    [ObservableProperty]
    private bool _requestSave;

    /// <summary>
    /// WebView2 에디터 테마. Behavior의 Theme에 바인딩된다.
    /// </summary>
    [ObservableProperty]
    private string _editorTheme;

    /// <summary>
    /// 파일이 열려있는지 여부. UI 상태 전환에 사용된다.
    /// </summary>
    public bool IsFileOpen => FilePath is not null;

    /// <summary>
    /// WebView2에 로드할 로컬 HTML 폴더 경로.
    /// </summary>
    public string WebViewFolder { get; }

    /// <summary>
    /// 기존 하네스 파일 목록 (홈 화면에 표시).
    /// </summary>
    public ObservableCollection<HarnessFileInfo> HarnessFiles { get; } = new();

    /// <summary>
    /// 생성 가능한 하네스 파일 목록 (홈 화면에 표시).
    /// </summary>
    public ObservableCollection<CreatableHarnessFile> CreatableFiles { get; } = new();

    public MarkdownEditorViewModel(
        IThemeService themeService,
        ITokenCounterService tokenCounter,
        IProjectContext projectContext,
        IHarnessScanner scanner)
    {
        _themeService = themeService;
        _tokenCounter = tokenCounter;
        _projectContext = projectContext;
        _scanner = scanner;
        _editorTheme = _themeService.IsDarkTheme ? "dark" : "light";

        var assemblyLocation = Path.GetDirectoryName(typeof(MarkdownEditorViewModel).Assembly.Location);
        if (assemblyLocation is null)
            throw new InvalidOperationException("에디터 어셈블리 경로를 찾을 수 없습니다.");

        WebViewFolder = Path.Combine(assemblyLocation, "WebView");

        IsActive = true;

        _ = LoadHomeAsync();
    }

    /// <inheritdoc />
    protected override void OnActivated()
    {
        Messenger.Register<ProjectPathChangedMessage>(this, (r, m) =>
        {
            if (!IsFileOpen)
            {
                _ = LoadHomeAsync();
            }
        });
    }

    /// <summary>
    /// 홈 화면 파일 목록을 로드한다.
    /// </summary>
    private async Task LoadHomeAsync()
    {
        var allFiles = new List<HarnessFileInfo>();

        var globalFiles = await _scanner.ScanAsync(_projectContext.GlobalPath, HarnessScope.Global);
        allFiles.AddRange(globalFiles);

        if (!string.IsNullOrEmpty(_projectContext.ProjectPath))
        {
            var projectFiles = await _scanner.ScanAsync(_projectContext.ProjectPath, HarnessScope.Project);
            allFiles.AddRange(projectFiles);
        }

        HarnessFiles.Clear();
        foreach (var file in allFiles)
        {
            HarnessFiles.Add(file);
        }

        // 생성 가능 목록
        var creatableFiles = new List<CreatableHarnessFile>();

        var globalCreatable = _scanner.GetCreatableFiles(
            _projectContext.GlobalPath, HarnessScope.Global, globalFiles);
        creatableFiles.AddRange(globalCreatable);

        if (!string.IsNullOrEmpty(_projectContext.ProjectPath))
        {
            var projectCreatable = _scanner.GetCreatableFiles(
                _projectContext.ProjectPath, HarnessScope.Project,
                allFiles.Where(f => f.Scope == HarnessScope.Project).ToList());
            creatableFiles.AddRange(projectCreatable);
        }

        CreatableFiles.Clear();
        foreach (var file in creatableFiles)
        {
            CreatableFiles.Add(file);
        }
    }

    /// <summary>
    /// 지정된 경로의 파일을 에디터에 로드한다.
    /// </summary>
    /// <param name="path">로드할 파일의 전체 경로.</param>
    public async Task LoadFileAsync(string path)
    {
        if (!File.Exists(path))
            return;

        FilePath = path;
        FileName = Path.GetFileName(path);
        FileType = Path.GetExtension(path).TrimStart('.');

        var content = await File.ReadAllTextAsync(path);
        _originalContent = content;
        _isNewFile = false;
        TokenCount = _tokenCounter.CountTokens(content);
        IsModified = false;

        MarkdownToLoad = content;
    }

    /// <summary>
    /// 홈 화면에서 기존 파일을 선택하여 에디터에 로드한다.
    /// </summary>
    [RelayCommand]
    private async Task SelectFileAsync(HarnessFileInfo? file)
    {
        if (file is null)
            return;

        await LoadFileAsync(file.FilePath);
    }

    /// <summary>
    /// 새 하네스 파일의 에디터를 빈 상태로 연다.
    /// 실제 파일은 저장 시에만 디스크에 생성된다.
    /// </summary>
    [RelayCommand]
    private Task CreateFileAsync(CreatableHarnessFile? template)
    {
        if (template is null)
            return Task.CompletedTask;

        var basePath = template.Scope == HarnessScope.Global
            ? _projectContext.GlobalPath
            : _projectContext.ProjectPath;

        if (basePath is null)
            return Task.CompletedTask;

        string fullPath;
        if (template.IsDirectory)
        {
            var extension = template.FileExtension ?? ".md";
            fullPath = Path.Combine(basePath, template.RelativePath, "new-file" + extension);
        }
        else
        {
            fullPath = Path.Combine(basePath, template.RelativePath);
        }

        FilePath = fullPath;
        FileName = Path.GetFileName(fullPath);
        FileType = Path.GetExtension(fullPath).TrimStart('.');
        _originalContent = string.Empty;
        _isNewFile = true;
        IsModified = false;
        TokenCount = 0;

        MarkdownToLoad = string.Empty;

        return Task.CompletedTask;
    }

    /// <summary>
    /// 편집 중인 파일을 닫고 홈 화면으로 복귀한다.
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsFileOpen))]
    private async Task CloseFileAsync()
    {
        FilePath = null;
        FileName = "에디터";
        FileType = null;
        _originalContent = null;
        _isNewFile = false;
        IsModified = false;
        TokenCount = 0;
        LineCount = 0;
        EditedMarkdown = null;

        await LoadHomeAsync();
    }

    partial void OnEditedMarkdownChanged(string? value)
    {
        if (value is null)
            return;

        IsModified = value != _originalContent;
        TokenCount = _tokenCounter.CountTokens(value);
    }

    /// <summary>
    /// 에디터 내용을 파일에 저장한다.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        RequestSave = true;
    }

    private bool CanSave() => FilePath is not null && (_isNewFile || IsModified);

    private bool CanRevert() => FilePath is not null && IsModified;

    partial void OnRequestSaveChanged(bool value)
    {
        if (!value && EditedMarkdown is not null && FilePath is not null)
        {
            _ = SaveToFileAsync();
        }
    }

    private async Task SaveToFileAsync()
    {
        if (FilePath is null || EditedMarkdown is null)
            return;

        var dir = Path.GetDirectoryName(FilePath);
        if (dir is not null)
        {
            Directory.CreateDirectory(dir);
        }

        await File.WriteAllTextAsync(FilePath, EditedMarkdown);
        _originalContent = EditedMarkdown;
        _isNewFile = false;
        IsModified = false;
    }

    /// <summary>
    /// 원본 내용으로 되돌린다.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRevert))]
    private void Revert()
    {
        if (_originalContent is null)
            return;

        MarkdownToLoad = _originalContent;
        IsModified = false;
    }
}
