using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HarnessHub.Abstract.Services;
using HarnessHub.Abstract.ViewModels;
using HarnessHub.Models.Harness;
using HarnessHub.Models.Messages;
using HarnessHub.Models.Preset;
using Serilog;

namespace HarnessHub.Preset.ViewModels;

/// <summary>
/// 프리셋 화면의 ViewModel. 목록/편집 상태를 관리하고 프리셋 CRUD를 수행한다.
/// </summary>
public partial class PresetViewModel : ObservableRecipient, IContentViewModel
{
    private readonly IPresetService _presetService;
    private readonly IProjectContext _projectContext;

    // --- View State ---

    [ObservableProperty]
    private PresetViewState _viewState = PresetViewState.List;

    [ObservableProperty]
    private bool _isLoading;

    // --- List State ---

    /// <summary>글로벌 프리셋 목록.</summary>
    public ObservableCollection<HarnessPreset> GlobalPresets { get; } = new();

    /// <summary>프로젝트 프리셋 목록.</summary>
    public ObservableCollection<HarnessPreset> ProjectPresets { get; } = new();

    [ObservableProperty]
    private string? _activeGlobalPresetName;

    [ObservableProperty]
    private string? _activeProjectPresetName;

    [ObservableProperty]
    private int _combinedTotalTokens;

    [ObservableProperty]
    private int _contextWindowSize = 200_000;

    [ObservableProperty]
    private double _combinedUsagePercentage;

    // --- Editor State ---

    [ObservableProperty]
    private string _editorName = string.Empty;

    [ObservableProperty]
    private string _editorDescription = string.Empty;

    [ObservableProperty]
    private HarnessScope _editorScope = HarnessScope.Global;

    [ObservableProperty]
    private bool _isEditing;

    /// <summary>편집 중인 프리셋의 파일 목록.</summary>
    public ObservableCollection<PresetFileEntry> EditorFiles { get; } = new();

    [ObservableProperty]
    private int _editorTotalTokens;

    private HarnessPreset? _editingPreset;

    public PresetViewModel(
        IPresetService presetService,
        IProjectContext projectContext)
    {
        _presetService = presetService;
        _projectContext = projectContext;

        IsActive = true;

        _ = LoadAsync();
    }

    /// <inheritdoc />
    protected override void OnActivated()
    {
        Messenger.Register<ProjectPathChangedMessage>(this, (r, m) =>
        {
            _ = LoadAsync();
        });
    }

    /// <summary>
    /// 모든 프리셋을 로드한다.
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            var globalPresets = await _presetService.LoadAllAsync(HarnessScope.Global);
            var projectPresets = await _presetService.LoadAllAsync(HarnessScope.Project);

            GlobalPresets.Clear();
            foreach (var preset in globalPresets)
            {
                GlobalPresets.Add(preset);
            }

            ProjectPresets.Clear();
            foreach (var preset in projectPresets)
            {
                ProjectPresets.Add(preset);
            }

            var activeState = await _presetService.GetActiveStateAsync();
            ActiveGlobalPresetName = activeState.GlobalPresetName;
            ActiveProjectPresetName = activeState.ProjectPresetName;

            UpdateCombinedTokens();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load presets");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 새 프리셋 생성 모드로 전환한다.
    /// </summary>
    [RelayCommand]
    private void NewPreset()
    {
        _editingPreset = null;
        IsEditing = false;
        EditorName = string.Empty;
        EditorDescription = string.Empty;
        EditorScope = HarnessScope.Global;
        EditorFiles.Clear();
        EditorTotalTokens = 0;
        ViewState = PresetViewState.Editor;
    }

    /// <summary>
    /// 기존 프리셋 편집 모드로 전환한다.
    /// </summary>
    [RelayCommand]
    private void EditPreset(HarnessPreset? preset)
    {
        if (preset is null)
        {
            return;
        }

        _editingPreset = preset;
        IsEditing = true;
        EditorName = preset.Name;
        EditorDescription = preset.Description;
        EditorScope = preset.Scope;

        EditorFiles.Clear();
        foreach (var file in preset.Files)
        {
            EditorFiles.Add(file);
        }

        EditorTotalTokens = preset.TotalTokens;
        ViewState = PresetViewState.Editor;
    }

    /// <summary>
    /// 프리셋을 삭제한다.
    /// </summary>
    [RelayCommand]
    private async Task DeletePresetAsync(HarnessPreset? preset)
    {
        if (preset is null)
        {
            return;
        }

        try
        {
            await _presetService.DeleteAsync(preset);

            var activeState = await _presetService.GetActiveStateAsync();
            if (preset.Scope == HarnessScope.Global &&
                string.Equals(activeState.GlobalPresetName, preset.Name, StringComparison.Ordinal))
            {
                await _presetService.SetActiveStateAsync(activeState with { GlobalPresetName = null });
            }
            else if (preset.Scope == HarnessScope.Project &&
                     string.Equals(activeState.ProjectPresetName, preset.Name, StringComparison.Ordinal))
            {
                await _presetService.SetActiveStateAsync(activeState with { ProjectPresetName = null });
            }

            await LoadAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete preset: {PresetName}", preset.Name);
        }
    }

    /// <summary>
    /// 프리셋을 대상 경로에 적용한다.
    /// </summary>
    [RelayCommand]
    private async Task ApplyPresetAsync(HarnessPreset? preset)
    {
        if (preset is null)
        {
            return;
        }

        var targetPath = preset.Scope == HarnessScope.Global
            ? _projectContext.GlobalPath
            : _projectContext.ProjectPath;

        if (string.IsNullOrEmpty(targetPath))
        {
            Log.Warning("Cannot apply project preset: no project path set");
            return;
        }

        IsLoading = true;

        try
        {
            await _presetService.ApplyAsync(preset, targetPath);

            var activeState = await _presetService.GetActiveStateAsync();
            var newState = preset.Scope == HarnessScope.Global
                ? activeState with { GlobalPresetName = preset.Name }
                : activeState with { ProjectPresetName = preset.Name };

            await _presetService.SetActiveStateAsync(newState);
            await LoadAsync();

            WeakReferenceMessenger.Default.Send(new PresetAppliedMessage(preset.Name, preset.Scope));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to apply preset: {PresetName}", preset.Name);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 에디터에서 프리셋을 저장한다.
    /// </summary>
    [RelayCommand]
    private async Task SavePresetAsync()
    {
        if (string.IsNullOrWhiteSpace(EditorName))
        {
            return;
        }

        IsLoading = true;

        try
        {
            var now = DateTime.UtcNow;
            var preset = new HarnessPreset
            {
                Name = EditorName.Trim(),
                Scope = EditorScope,
                Description = EditorDescription,
                Files = EditorFiles.ToList(),
                TotalTokens = EditorTotalTokens,
                CreatedAt = _editingPreset?.CreatedAt ?? now,
                UpdatedAt = now,
                FolderPath = _editingPreset?.FolderPath ?? string.Empty
            };

            var sourceFilePaths = _editingPreset is not null
                ? _editingPreset.Files
                    .Select(f => Path.Combine(_editingPreset.FolderPath, f.RelativePath))
                    .Where(File.Exists)
                    .ToList()
                : new List<string>();

            await _presetService.SaveAsync(preset, sourceFilePaths);
            await LoadAsync();
            ViewState = PresetViewState.List;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save preset");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 편집을 취소하고 목록으로 돌아간다.
    /// </summary>
    [RelayCommand]
    private void CancelEdit()
    {
        ViewState = PresetViewState.List;
    }

    /// <summary>
    /// 프리셋을 JSON 번들로 내보낸다.
    /// </summary>
    [RelayCommand]
    private async Task ExportPresetAsync(HarnessPreset? preset)
    {
        if (preset is null)
        {
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "프리셋 내보내기",
            FileName = $"{preset.Name}.json",
            Filter = "JSON 파일 (*.json)|*.json"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            await _presetService.ExportAsync(preset, dialog.FileName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to export preset: {PresetName}", preset.Name);
        }
    }

    /// <summary>
    /// JSON 번들에서 프리셋을 가져온다.
    /// </summary>
    [RelayCommand]
    private async Task ImportPresetAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "프리셋 가져오기",
            Filter = "JSON 파일 (*.json)|*.json"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            await _presetService.ImportAsync(dialog.FileName, HarnessScope.Project);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to import preset from {Path}", dialog.FileName);
        }
    }

    /// <summary>
    /// 현재 하네스 구성을 프리셋으로 캡처한다.
    /// </summary>
    [RelayCommand]
    private async Task CaptureCurrentAsync(HarnessScope scope)
    {
        var sourcePath = scope == HarnessScope.Global
            ? _projectContext.GlobalPath
            : _projectContext.ProjectPath;

        if (string.IsNullOrEmpty(sourcePath))
        {
            Log.Warning("Cannot capture: no {Scope} path set", scope);
            return;
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmm");
        var name = scope == HarnessScope.Global
            ? $"글로벌-캡처-{timestamp}"
            : $"프로젝트-캡처-{timestamp}";

        IsLoading = true;

        try
        {
            await _presetService.CaptureCurrentAsync(name, "현재 하네스 구성 캡처", sourcePath, scope);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to capture current harness as preset");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateCombinedTokens()
    {
        var globalTokens = GlobalPresets
            .Where(p => string.Equals(p.Name, ActiveGlobalPresetName, StringComparison.Ordinal))
            .Select(p => p.TotalTokens)
            .FirstOrDefault();

        var projectTokens = ProjectPresets
            .Where(p => string.Equals(p.Name, ActiveProjectPresetName, StringComparison.Ordinal))
            .Select(p => p.TotalTokens)
            .FirstOrDefault();

        CombinedTotalTokens = globalTokens + projectTokens;
        CombinedUsagePercentage = ContextWindowSize > 0
            ? (double)CombinedTotalTokens / ContextWindowSize * 100
            : 0;
    }
}
