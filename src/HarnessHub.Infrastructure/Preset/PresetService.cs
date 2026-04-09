using System.Text.Json;
using System.Text.Json.Serialization;
using HarnessHub.Abstract.Services;
using HarnessHub.Models.Harness;
using HarnessHub.Models.Preset;
using Serilog;

namespace HarnessHub.Infrastructure.Preset;

/// <summary>
/// 프리셋 CRUD, 적용, 내보내기/가져오기를 구현한다.
/// 프리셋은 %AppData%/HarnessHub/presets/{global,project}/{preset-name}/ 폴더에 저장된다.
/// </summary>
public sealed class PresetService : IPresetService
{
    private readonly IHarnessScanner _scanner;
    private readonly ITokenCounterService _tokenCounter;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static string BasePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "HarnessHub", "presets");

    private static string BackupBasePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "HarnessHub", "backups");

    private static string ActiveStatePath => Path.Combine(BasePath, "active.json");

    public PresetService(IHarnessScanner scanner, ITokenCounterService tokenCounter)
    {
        _scanner = scanner;
        _tokenCounter = tokenCounter;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HarnessPreset>> LoadAllAsync(HarnessScope scope)
    {
        var scopeDir = GetScopeDirectory(scope);
        var results = new List<HarnessPreset>();

        if (!Directory.Exists(scopeDir))
        {
            return Task.FromResult<IReadOnlyList<HarnessPreset>>(results);
        }

        foreach (var presetDir in Directory.EnumerateDirectories(scopeDir))
        {
            var metadataPath = Path.Combine(presetDir, "preset.json");
            if (!File.Exists(metadataPath))
            {
                continue;
            }

            try
            {
                var json = File.ReadAllText(metadataPath);
                var metadata = JsonSerializer.Deserialize<PresetMetadata>(json, JsonOptions);
                if (metadata is null)
                {
                    continue;
                }

                results.Add(MapToPreset(metadata, presetDir));
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to load preset from {PresetDir}", presetDir);
            }
        }

        return Task.FromResult<IReadOnlyList<HarnessPreset>>(results);
    }

    /// <inheritdoc />
    public Task SaveAsync(HarnessPreset preset, IReadOnlyList<string> sourceFilePaths)
    {
        var presetDir = GetPresetDirectory(preset.Scope, preset.Name);
        Directory.CreateDirectory(presetDir);

        var entries = new List<PresetFileEntryDto>();
        var totalTokens = 0;

        foreach (var sourcePath in sourceFilePaths)
        {
            if (!File.Exists(sourcePath))
            {
                Log.Warning("Source file not found, skipping: {SourcePath}", sourcePath);
                continue;
            }

            var entry = CopyFileToPreset(sourcePath, presetDir, preset.Scope);
            entries.Add(entry);
            totalTokens += entry.TokenCount;
        }

        var now = DateTime.UtcNow;
        var metadata = new PresetMetadata
        {
            Name = preset.Name,
            Scope = preset.Scope.ToString(),
            Description = preset.Description,
            Files = entries,
            TotalTokens = totalTokens,
            CreatedAt = preset.CreatedAt == default ? now : preset.CreatedAt,
            UpdatedAt = now
        };

        var json = JsonSerializer.Serialize(metadata, JsonOptions);
        File.WriteAllText(Path.Combine(presetDir, "preset.json"), json);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync(HarnessPreset preset)
    {
        if (Directory.Exists(preset.FolderPath))
        {
            Directory.Delete(preset.FolderPath, recursive: true);
            Log.Information("Deleted preset: {PresetName} at {Path}", preset.Name, preset.FolderPath);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ApplyAsync(HarnessPreset preset, string targetPath, bool backup = true)
    {
        if (!Directory.Exists(preset.FolderPath))
        {
            throw new DirectoryNotFoundException($"Preset folder not found: {preset.FolderPath}");
        }

        if (backup)
        {
            BackupExistingFiles(targetPath, preset.Scope);
        }

        foreach (var file in preset.Files)
        {
            var sourcePath = Path.Combine(preset.FolderPath, file.RelativePath);
            var destPath = Path.Combine(targetPath, file.RelativePath);

            if (!File.Exists(sourcePath))
            {
                Log.Warning("Preset file missing, skipping: {SourcePath}", sourcePath);
                continue;
            }

            var destDir = Path.GetDirectoryName(destPath);
            if (destDir is not null)
            {
                Directory.CreateDirectory(destDir);
            }

            File.Copy(sourcePath, destPath, overwrite: true);
        }

        Log.Information("Applied preset: {PresetName} to {TargetPath}", preset.Name, targetPath);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ExportAsync(HarnessPreset preset, string exportPath)
    {
        var fileContents = new Dictionary<string, string>();

        foreach (var file in preset.Files)
        {
            var filePath = Path.Combine(preset.FolderPath, file.RelativePath);
            if (File.Exists(filePath))
            {
                fileContents[file.RelativePath] = File.ReadAllText(filePath);
            }
        }

        var metadataPath = Path.Combine(preset.FolderPath, "preset.json");
        PresetMetadata? metadata = null;
        if (File.Exists(metadataPath))
        {
            var metaJson = File.ReadAllText(metadataPath);
            metadata = JsonSerializer.Deserialize<PresetMetadata>(metaJson, JsonOptions);
        }

        if (metadata is null)
        {
            throw new InvalidOperationException($"Cannot read preset metadata from {metadataPath}");
        }

        var bundle = new PresetBundle
        {
            Metadata = metadata,
            FileContents = fileContents,
            ExportedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(bundle, JsonOptions);
        File.WriteAllText(exportPath, json);

        Log.Information("Exported preset: {PresetName} to {ExportPath}", preset.Name, exportPath);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<HarnessPreset> ImportAsync(string jsonBundlePath, HarnessScope scope)
    {
        if (!File.Exists(jsonBundlePath))
        {
            throw new FileNotFoundException("Import bundle file not found", jsonBundlePath);
        }

        var json = File.ReadAllText(jsonBundlePath);
        var bundle = JsonSerializer.Deserialize<PresetBundle>(json, JsonOptions);
        if (bundle is null)
        {
            throw new InvalidOperationException("Failed to deserialize preset bundle");
        }

        var presetName = bundle.Metadata.Name;
        var presetDir = GetPresetDirectory(scope, presetName);
        Directory.CreateDirectory(presetDir);

        foreach (var (relativePath, content) in bundle.FileContents)
        {
            var filePath = Path.Combine(presetDir, relativePath);
            var fileDir = Path.GetDirectoryName(filePath);
            if (fileDir is not null)
            {
                Directory.CreateDirectory(fileDir);
            }

            File.WriteAllText(filePath, content);
        }

        var updatedMetadata = bundle.Metadata with
        {
            Scope = scope.ToString(),
            UpdatedAt = DateTime.UtcNow
        };

        var metaJson = JsonSerializer.Serialize(updatedMetadata, JsonOptions);
        File.WriteAllText(Path.Combine(presetDir, "preset.json"), metaJson);

        var preset = MapToPreset(updatedMetadata, presetDir);
        Log.Information("Imported preset: {PresetName} from {BundlePath}", presetName, jsonBundlePath);

        return Task.FromResult(preset);
    }

    /// <inheritdoc />
    public async Task<HarnessPreset> CaptureCurrentAsync(
        string name,
        string description,
        string sourcePath,
        HarnessScope scope)
    {
        var harnessFiles = await _scanner.ScanAsync(sourcePath, scope);

        var presetDir = GetPresetDirectory(scope, name);
        Directory.CreateDirectory(presetDir);

        var entries = new List<PresetFileEntryDto>();
        var totalTokens = 0;

        foreach (var harnessFile in harnessFiles)
        {
            var relativePath = Path.GetRelativePath(sourcePath, harnessFile.FilePath);
            var destPath = Path.Combine(presetDir, relativePath);

            var destDir = Path.GetDirectoryName(destPath);
            if (destDir is not null)
            {
                Directory.CreateDirectory(destDir);
            }

            File.Copy(harnessFile.FilePath, destPath, overwrite: true);

            entries.Add(new PresetFileEntryDto
            {
                RelativePath = relativePath,
                FileType = harnessFile.FileType.ToString(),
                TokenCount = harnessFile.TokenCount
            });

            totalTokens += harnessFile.TokenCount;
        }

        var now = DateTime.UtcNow;
        var metadata = new PresetMetadata
        {
            Name = name,
            Scope = scope.ToString(),
            Description = description,
            Files = entries,
            TotalTokens = totalTokens,
            CreatedAt = now,
            UpdatedAt = now
        };

        var json = JsonSerializer.Serialize(metadata, JsonOptions);
        File.WriteAllText(Path.Combine(presetDir, "preset.json"), json);

        Log.Information("Captured current harness as preset: {PresetName} from {SourcePath}", name, sourcePath);
        return MapToPreset(metadata, presetDir);
    }

    /// <inheritdoc />
    public Task<ActivePresetState> GetActiveStateAsync()
    {
        if (!File.Exists(ActiveStatePath))
        {
            return Task.FromResult(new ActivePresetState());
        }

        try
        {
            var json = File.ReadAllText(ActiveStatePath);
            var state = JsonSerializer.Deserialize<ActivePresetState>(json, JsonOptions);
            return Task.FromResult(state ?? new ActivePresetState());
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load active preset state");
            return Task.FromResult(new ActivePresetState());
        }
    }

    /// <inheritdoc />
    public Task SetActiveStateAsync(ActivePresetState state)
    {
        try
        {
            Directory.CreateDirectory(BasePath);
            var json = JsonSerializer.Serialize(state, JsonOptions);
            File.WriteAllText(ActiveStatePath, json);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to save active preset state");
        }

        return Task.CompletedTask;
    }

    private static string GetScopeDirectory(HarnessScope scope)
    {
        var scopeName = scope == HarnessScope.Global ? "global" : "project";
        return Path.Combine(BasePath, scopeName);
    }

    private static string GetPresetDirectory(HarnessScope scope, string presetName)
    {
        var sanitized = SanitizeFolderName(presetName);
        return Path.Combine(GetScopeDirectory(scope), sanitized);
    }

    private static string SanitizeFolderName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Select(c => invalidChars.Contains(c) ? '-' : c).ToArray());
        return sanitized.Trim().TrimEnd('.');
    }

    private PresetFileEntryDto CopyFileToPreset(string sourcePath, string presetDir, HarnessScope scope)
    {
        var fileName = Path.GetFileName(sourcePath);
        var destPath = Path.Combine(presetDir, fileName);

        var destDir = Path.GetDirectoryName(destPath);
        if (destDir is not null)
        {
            Directory.CreateDirectory(destDir);
        }

        File.Copy(sourcePath, destPath, overwrite: true);

        var content = ReadFileSafe(sourcePath);
        var tokenCount = content is not null ? _tokenCounter.CountTokens(content) : 0;

        return new PresetFileEntryDto
        {
            RelativePath = fileName,
            FileType = GuessFileType(sourcePath).ToString(),
            TokenCount = tokenCount
        };
    }

    private static HarnessFileType GuessFileType(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        return fileName.ToLowerInvariant() switch
        {
            "claude.md" => HarnessFileType.ClaudeMd,
            "claude.local.md" => HarnessFileType.ClaudeLocalMd,
            "settings.json" when filePath.Contains(".claude") => HarnessFileType.ClaudeSettings,
            "settings.local.json" => HarnessFileType.ClaudeSettingsLocal,
            ".mcp.json" => HarnessFileType.McpConfig,
            "agents.md" => HarnessFileType.AgentsMd,
            ".env" => HarnessFileType.EnvConfig,
            _ when filePath.Contains("rules") => HarnessFileType.ClaudeRules,
            _ when filePath.Contains("agents") => HarnessFileType.AgentDefinition,
            _ when filePath.Contains("memory") => HarnessFileType.Memory,
            _ => HarnessFileType.ClaudeMd
        };
    }

    private void BackupExistingFiles(string targetPath, HarnessScope scope)
    {
        try
        {
            var existingFiles = _scanner.ScanAsync(targetPath, scope).GetAwaiter().GetResult();
            if (existingFiles.Count == 0)
            {
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            var backupDir = Path.Combine(BackupBasePath, timestamp);
            Directory.CreateDirectory(backupDir);

            foreach (var file in existingFiles)
            {
                var relativePath = Path.GetRelativePath(targetPath, file.FilePath);
                var backupPath = Path.Combine(backupDir, relativePath);

                var backupFileDir = Path.GetDirectoryName(backupPath);
                if (backupFileDir is not null)
                {
                    Directory.CreateDirectory(backupFileDir);
                }

                File.Copy(file.FilePath, backupPath, overwrite: true);
            }

            Log.Information("Backed up {Count} files to {BackupDir}", existingFiles.Count, backupDir);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to backup existing files at {TargetPath}", targetPath);
        }
    }

    private static HarnessPreset MapToPreset(PresetMetadata metadata, string folderPath)
    {
        var files = metadata.Files.Select(f => new PresetFileEntry
        {
            RelativePath = f.RelativePath,
            FileType = Enum.TryParse<HarnessFileType>(f.FileType, out var ft) ? ft : HarnessFileType.ClaudeMd,
            TokenCount = f.TokenCount
        }).ToList();

        return new HarnessPreset
        {
            Name = metadata.Name,
            Scope = Enum.TryParse<HarnessScope>(metadata.Scope, out var scope) ? scope : HarnessScope.Project,
            Description = metadata.Description,
            Files = files,
            TotalTokens = metadata.TotalTokens,
            CreatedAt = metadata.CreatedAt,
            UpdatedAt = metadata.UpdatedAt,
            FolderPath = folderPath
        };
    }

    private static string? ReadFileSafe(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to read file: {Path}", path);
            return null;
        }
    }
}
