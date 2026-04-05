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

    private static readonly Dictionary<string, (HarnessFileType Type, HarnessLever Lever)> GlobalPatterns = new()
    {
        ["CLAUDE.md"] = (HarnessFileType.ClaudeMd, HarnessLever.SystemPrompt),
        ["settings.json"] = (HarnessFileType.ClaudeSettings, HarnessLever.Hook),
    };

    private static readonly Dictionary<string, (HarnessFileType Type, HarnessLever Lever)> ProjectPatterns = new()
    {
        ["CLAUDE.md"] = (HarnessFileType.ClaudeMd, HarnessLever.SystemPrompt),
        ["CLAUDE.local.md"] = (HarnessFileType.ClaudeLocalMd, HarnessLever.SystemPrompt),
        [".claude/settings.json"] = (HarnessFileType.ClaudeSettings, HarnessLever.Hook),
        [".claude/settings.local.json"] = (HarnessFileType.ClaudeSettingsLocal, HarnessLever.Hook),
        [".mcp.json"] = (HarnessFileType.McpConfig, HarnessLever.McpServer),
        ["AGENTS.md"] = (HarnessFileType.AgentsMd, HarnessLever.SubAgent),
        [".cursorrules"] = (HarnessFileType.CursorRulesLegacy, HarnessLever.SystemPrompt),
        [".github/copilot-instructions.md"] = (HarnessFileType.CopilotInstructions, HarnessLever.SystemPrompt),
        [".env"] = (HarnessFileType.EnvConfig, HarnessLever.SystemPrompt),
    };

    public HarnessScanner(ITokenCounterService tokenCounter)
    {
        _tokenCounter = tokenCounter;
    }

    public Task<IReadOnlyList<HarnessFileInfo>> ScanAsync(string folderPath, HarnessScope scope)
    {
        var results = new List<HarnessFileInfo>();

        if (!Directory.Exists(folderPath))
        {
            Log.Warning("Scan target folder does not exist: {FolderPath}", folderPath);
            return Task.FromResult<IReadOnlyList<HarnessFileInfo>>(results);
        }

        var patterns = scope == HarnessScope.Global ? GlobalPatterns : ProjectPatterns;

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

    private void ScanWildcardPatterns(string folderPath, HarnessScope scope, List<HarnessFileInfo> results)
    {
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
            ScanDirectory(folderPath, Path.Combine(".cursor", "rules"), "*.mdc", HarnessFileType.CursorRules, HarnessLever.SystemPrompt, scope, results);
            ScanDirectory(folderPath, Path.Combine(".windsurf", "rules"), "*.md", HarnessFileType.WindsurfRules, HarnessLever.SystemPrompt, scope, results);
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
