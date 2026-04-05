namespace HarnessHub.Models.Harness;

/// <summary>
/// 하네스 파일의 유형을 나타낸다.
/// </summary>
public enum HarnessFileType
{
    ClaudeMd,
    ClaudeLocalMd,
    ClaudeSettings,
    ClaudeSettingsLocal,
    ClaudeRules,
    AgentDefinition,
    McpConfig,
    CursorRules,
    CursorRulesLegacy,
    WindsurfRules,
    CopilotInstructions,
    AgentsMd,
    Memory,
    EnvConfig,
    ClaudeMdSub
}
