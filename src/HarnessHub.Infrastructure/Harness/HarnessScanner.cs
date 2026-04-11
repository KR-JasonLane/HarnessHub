using HarnessHub.Abstract.Services;
using HarnessHub.Models.Harness;
using Serilog;

namespace HarnessHub.Infrastructure.Harness;

/// <summary>
/// 폴더를 스캔하여 하네스 파일을 자동 감지한다.
/// </summary>
public sealed class HarnessScanner : IHarnessScanner
{
    private readonly ITokenCounterService _tokenCounter;
    private readonly IAppSettingsService _appSettings;

    // === Claude Code 패턴 ===
    private static readonly Dictionary<string, (HarnessFileType Type, HarnessLever Lever)> ClaudeGlobalPatterns = new()
    {
        ["CLAUDE.md"] = (HarnessFileType.ClaudeMd, HarnessLever.SystemPrompt),
        ["settings.json"] = (HarnessFileType.ClaudeSettings, HarnessLever.Hook),
    };

    private static readonly Dictionary<string, (HarnessFileType Type, HarnessLever Lever)> ClaudeProjectPatterns = new()
    {
        ["CLAUDE.md"] = (HarnessFileType.ClaudeMd, HarnessLever.SystemPrompt),
        ["CLAUDE.local.md"] = (HarnessFileType.ClaudeLocalMd, HarnessLever.SystemPrompt),
        [".claude/settings.json"] = (HarnessFileType.ClaudeSettings, HarnessLever.Hook),
        [".claude/settings.local.json"] = (HarnessFileType.ClaudeSettingsLocal, HarnessLever.Hook),
        [".mcp.json"] = (HarnessFileType.McpConfig, HarnessLever.McpServer),
        ["AGENTS.md"] = (HarnessFileType.AgentsMd, HarnessLever.SubAgent),
        [".env"] = (HarnessFileType.EnvConfig, HarnessLever.SystemPrompt),
    };

    // === Cursor 패턴 (글로벌 하네스 없음, 프로젝트만) ===
    private static readonly Dictionary<string, (HarnessFileType Type, HarnessLever Lever)> CursorProjectPatterns = new()
    {
        [".cursorrules"] = (HarnessFileType.ClaudeMd, HarnessLever.SystemPrompt),
    };

    public HarnessScanner(ITokenCounterService tokenCounter, IAppSettingsService appSettings)
    {
        _tokenCounter = tokenCounter;
        _appSettings = appSettings;
    }

    public Task<IReadOnlyList<HarnessFileInfo>> ScanAsync(string folderPath, HarnessScope scope)
    {
        var results = new List<HarnessFileInfo>();

        if (!Directory.Exists(folderPath))
        {
            Log.Warning("Scan target folder does not exist: {FolderPath}", folderPath);
            return Task.FromResult<IReadOnlyList<HarnessFileInfo>>(results);
        }

        var patterns = GetPatterns(scope);

        foreach (var (relativePath, meta) in patterns)
        {
            var fullPath = Path.Combine(folderPath, relativePath);
            if (File.Exists(fullPath))
            {
                results.Add(CreateFileInfo(fullPath, meta.Type, scope, meta.Lever));
            }
        }

        ScanWildcardPatterns(folderPath, scope, results);

        return Task.FromResult<IReadOnlyList<HarnessFileInfo>>(results);
    }

    private static readonly Dictionary<string, (HarnessFileType, HarnessLever)> EmptyPatterns = new();

    private Dictionary<string, (HarnessFileType Type, HarnessLever Lever)> GetPatterns(HarnessScope scope)
    {
        return _appSettings.ActiveProvider switch
        {
            HarnessProvider.Cursor => scope == HarnessScope.Global
                ? EmptyPatterns
                : CursorProjectPatterns,
            _ => scope == HarnessScope.Global
                ? ClaudeGlobalPatterns
                : ClaudeProjectPatterns
        };
    }

    private void ScanWildcardPatterns(string folderPath, HarnessScope scope, List<HarnessFileInfo> results)
    {
        if (_appSettings.ActiveProvider == HarnessProvider.Cursor)
        {
            if (scope == HarnessScope.Project)
            {
                ScanDirectory(folderPath, Path.Combine(".cursor", "rules"), "*.mdc",
                    HarnessFileType.ClaudeRules, HarnessLever.SystemPrompt, scope, results);
            }
            return;
        }

        // Claude Code 와일드카드 패턴
        if (scope == HarnessScope.Global)
        {
            ScanDirectory(folderPath, "rules", "*.md", HarnessFileType.ClaudeRules, HarnessLever.SystemPrompt, scope, results);
            ScanDirectory(folderPath, "agents", "*.md", HarnessFileType.AgentDefinition, HarnessLever.SubAgent, scope, results);

            var memoryDir = Path.Combine(folderPath, "projects");
            if (Directory.Exists(memoryDir))
            {
                foreach (var mdFile in Directory.EnumerateFiles(memoryDir, "MEMORY.md", SearchOption.AllDirectories))
                {
                    results.Add(CreateFileInfo(mdFile, HarnessFileType.Memory, scope, HarnessLever.SystemPrompt));
                }
            }
        }
        else
        {
            ScanDirectory(folderPath, Path.Combine(".claude", "rules"), "*.md", HarnessFileType.ClaudeRules, HarnessLever.SystemPrompt, scope, results);
            ScanDirectory(folderPath, Path.Combine(".claude", "agents"), "*.md", HarnessFileType.AgentDefinition, HarnessLever.SubAgent, scope, results);
        }
    }

    private void ScanDirectory(
        string basePath,
        string subDir,
        string searchPattern,
        HarnessFileType fileType,
        HarnessLever lever,
        HarnessScope scope,
        List<HarnessFileInfo> results)
    {
        var dirPath = Path.Combine(basePath, subDir);
        if (!Directory.Exists(dirPath))
            return;

        foreach (var file in Directory.EnumerateFiles(dirPath, searchPattern))
        {
            results.Add(CreateFileInfo(file, fileType, scope, lever));
        }
    }

    private HarnessFileInfo CreateFileInfo(
        string fullPath,
        HarnessFileType fileType,
        HarnessScope scope,
        HarnessLever lever)
    {
        var fileInfo = new FileInfo(fullPath);
        var content = ReadFileSafe(fullPath);
        var tokenCount = content is not null ? _tokenCounter.CountTokens(content) : 0;

        return new HarnessFileInfo
        {
            FilePath = fullPath,
            FileName = fileInfo.Name,
            FileType = fileType,
            Scope = scope,
            Lever = lever,
            TokenCount = tokenCount,
            LastModified = fileInfo.LastWriteTime
        };
    }

    public IReadOnlyList<CreatableHarnessFile> GetCreatableFiles(
        string folderPath,
        HarnessScope scope,
        IReadOnlyList<HarnessFileInfo> existingFiles)
    {
        var results = new List<CreatableHarnessFile>();
        var existingPaths = new HashSet<string>(
            existingFiles
                .Where(f => f.Scope == scope)
                .Select(f => f.FilePath),
            StringComparer.OrdinalIgnoreCase);

        // 고정 패턴에서 미생성 파일 추출
        var patterns = GetPatterns(scope);
        foreach (var (relativePath, meta) in patterns)
        {
            var fullPath = Path.Combine(folderPath, relativePath);
            if (!existingPaths.Contains(fullPath))
            {
                results.Add(new CreatableHarnessFile
                {
                    RelativePath = relativePath,
                    FileType = meta.Type,
                    Lever = meta.Lever,
                    Scope = scope,
                    Description = GetFileDescription(meta.Type)
                });
            }
        }

        // 와일드카드 디렉토리 (항상 추가 가능)
        if (scope == HarnessScope.Global)
        {
            results.Add(CreateDirectoryEntry("rules/", "*.md", ".md",
                HarnessFileType.ClaudeRules, HarnessLever.SystemPrompt, scope));
            results.Add(CreateDirectoryEntry("agents/", "*.md", ".md",
                HarnessFileType.AgentDefinition, HarnessLever.SubAgent, scope));
        }
        else
        {
            results.Add(CreateDirectoryEntry(".claude/rules/", "*.md", ".md",
                HarnessFileType.ClaudeRules, HarnessLever.SystemPrompt, scope));
            results.Add(CreateDirectoryEntry(".claude/agents/", "*.md", ".md",
                HarnessFileType.AgentDefinition, HarnessLever.SubAgent, scope));
        }

        return results;
    }

    private static CreatableHarnessFile CreateDirectoryEntry(
        string relativePath,
        string searchPattern,
        string extension,
        HarnessFileType fileType,
        HarnessLever lever,
        HarnessScope scope)
    {
        return new CreatableHarnessFile
        {
            RelativePath = relativePath,
            FileType = fileType,
            Lever = lever,
            Scope = scope,
            Description = GetFileDescription(fileType) + " 추가",
            IsDirectory = true,
            FileExtension = extension
        };
    }

    private static string GetFileDescription(HarnessFileType fileType) => fileType switch
    {
        HarnessFileType.ClaudeMd => "Claude 시스템 프롬프트",
        HarnessFileType.ClaudeLocalMd => "Claude 로컬 프롬프트",
        HarnessFileType.ClaudeSettings => "Claude 설정",
        HarnessFileType.ClaudeSettingsLocal => "Claude 로컬 설정",
        HarnessFileType.ClaudeRules => "Claude 규칙",
        HarnessFileType.AgentDefinition => "에이전트 정의",
        HarnessFileType.McpConfig => "MCP 서버 설정",
        HarnessFileType.AgentsMd => "에이전트 정의 (AGENTS.md)",
        HarnessFileType.Memory => "메모리",
        HarnessFileType.EnvConfig => "환경 변수",
        _ => fileType.ToString()
    };

    private static string? ReadFileSafe(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to read file for token counting: {Path}", path);
            return null;
        }
    }
}
