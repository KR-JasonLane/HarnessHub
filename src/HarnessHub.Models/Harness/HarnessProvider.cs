namespace HarnessHub.Models.Harness;

/// <summary>
/// 지원하는 하네스 프로바이더 유형.
/// </summary>
public enum HarnessProvider
{
    /// <summary>Claude Code (CLAUDE.md, settings.json, rules/, agents/, .mcp.json 등)</summary>
    ClaudeCode,

    /// <summary>Cursor (.cursorrules, .cursor/rules/*.mdc)</summary>
    Cursor
}
