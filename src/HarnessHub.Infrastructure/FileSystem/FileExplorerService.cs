using HarnessHub.Abstract.Services;
using HarnessHub.Models.Explorer;
using HarnessHub.Models.Harness;
using Serilog;

namespace HarnessHub.Infrastructure.FileSystem;

/// <summary>
/// 폴더 구조를 스캔하여 FolderNode 트리를 생성한다.
/// 하네스 관련 폴더는 깊이 스캔, 일반 폴더는 1단계만 표시한다.
/// </summary>
public sealed class FileExplorerService : IFileExplorerService
{
    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", "node_modules", "bin", "obj", ".vs", ".idea",
        "__pycache__", ".venv", "dist", "build", "packages"
    };

    private static readonly HashSet<string> HarnessDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".claude", ".cursor", ".windsurf", ".github", "hooks", "skills"
    };

    public Task<FolderNode> BuildFolderTreeAsync(string rootPath, IReadOnlyList<HarnessFileInfo> harnessFiles)
    {
        var harnessPaths = new HashSet<string>(
            harnessFiles.Select(f => Path.GetFullPath(f.FilePath)),
            StringComparer.OrdinalIgnoreCase);

        var root = BuildNode(rootPath, harnessPaths, depth: 0, maxDepth: 1);
        root.IsExpanded = true;

        return Task.FromResult(root);
    }

    private static FolderNode BuildNode(string path, HashSet<string> harnessPaths, int depth, int maxDepth)
    {
        var dirInfo = new DirectoryInfo(path);

        var node = new FolderNode
        {
            Name = dirInfo.Name,
            FullPath = dirInfo.FullName,
            IsDirectory = true,
            IsExpanded = depth == 0,
            Children = new List<FolderNode>()
        };

        try
        {
            foreach (var dir in dirInfo.EnumerateDirectories().OrderBy(d => d.Name))
            {
                if (IgnoredDirectories.Contains(dir.Name))
                    continue;

                var isHarnessDir = HarnessDirectories.Contains(dir.Name);
                var childMaxDepth = isHarnessDir ? 5 : maxDepth;

                if (depth < childMaxDepth)
                {
                    var childNode = BuildNode(dir.FullName, harnessPaths, depth + 1, childMaxDepth);
                    childNode.IsExpanded = isHarnessDir;
                    node.Children.Add(childNode);
                }
                else
                {
                    node.Children.Add(new FolderNode
                    {
                        Name = dir.Name,
                        FullPath = dir.FullName,
                        IsDirectory = true
                    });
                }
            }

            foreach (var file in dirInfo.EnumerateFiles().OrderBy(f => f.Name))
            {
                var fullPath = Path.GetFullPath(file.FullName);
                var isHarness = harnessPaths.Contains(fullPath);
                var harnessType = isHarness
                    ? GetHarnessFileType(file.Name)
                    : (HarnessFileType?)null;

                node.Children.Add(new FolderNode
                {
                    Name = file.Name,
                    FullPath = file.FullName,
                    IsDirectory = false,
                    IsHarnessFile = isHarness,
                    HarnessFileType = harnessType
                });
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Warning(ex, "Access denied: {Path}", path);
        }

        return node;
    }

    private static HarnessFileType? GetHarnessFileType(string fileName)
    {
        return fileName.ToLowerInvariant() switch
        {
            "claude.md" => Models.Harness.HarnessFileType.ClaudeMd,
            "claude.local.md" => Models.Harness.HarnessFileType.ClaudeLocalMd,
            "agents.md" => Models.Harness.HarnessFileType.AgentsMd,
            "settings.json" => Models.Harness.HarnessFileType.ClaudeSettings,
            "settings.local.json" => Models.Harness.HarnessFileType.ClaudeSettingsLocal,
            ".mcp.json" => Models.Harness.HarnessFileType.McpConfig,
            ".cursorrules" => Models.Harness.HarnessFileType.CursorRulesLegacy,
            ".env" => Models.Harness.HarnessFileType.EnvConfig,
            "copilot-instructions.md" => Models.Harness.HarnessFileType.CopilotInstructions,
            "memory.md" => Models.Harness.HarnessFileType.Memory,
            _ when fileName.EndsWith(".mdc", StringComparison.OrdinalIgnoreCase)
                => Models.Harness.HarnessFileType.CursorRules,
            _ => null
        };
    }
}
