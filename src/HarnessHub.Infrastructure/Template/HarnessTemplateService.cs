using HarnessHub.Abstract.Services;
using HarnessHub.Models.Harness;

namespace HarnessHub.Infrastructure.Template;

/// <summary>
/// 하네스 파일 유형별 기본 템플릿 콘텐츠를 제공하는 서비스.
/// </summary>
public sealed class HarnessTemplateService : IHarnessTemplateService
{
    /// <inheritdoc />
    public string GetTemplate(HarnessFileType fileType) => fileType switch
    {
        HarnessFileType.ClaudeMd => ClaudeMdTemplate,
        HarnessFileType.ClaudeLocalMd => ClaudeLocalMdTemplate,
        HarnessFileType.ClaudeSettings => ClaudeSettingsTemplate,
        HarnessFileType.ClaudeSettingsLocal => ClaudeSettingsLocalTemplate,
        HarnessFileType.ClaudeRules => ClaudeRulesTemplate,
        HarnessFileType.AgentDefinition => AgentDefinitionTemplate,
        HarnessFileType.McpConfig => McpConfigTemplate,
        HarnessFileType.AgentsMd => AgentsMdTemplate,
        HarnessFileType.EnvConfig => EnvConfigTemplate,
        HarnessFileType.Memory => MemoryTemplate,
        _ => string.Empty
    };

    private const string ClaudeMdTemplate =
        """
        # Project Instructions

        ## Overview
        <!-- 프로젝트 목적과 컨텍스트를 설명하세요 -->

        ## Architecture
        <!-- 아키텍처 규칙과 레이어 구조를 정의하세요 -->

        ## Coding Rules
        <!-- 코딩 컨벤션과 금지 패턴을 명시하세요 -->

        ## Testing
        <!-- 테스트 전략과 규칙을 정의하세요 -->
        """;

    private const string ClaudeLocalMdTemplate =
        """
        # Local Instructions

        <!-- 이 파일은 Git에 커밋되지 않는 로컬 전용 지침입니다 -->
        <!-- 개인 환경에 맞는 추가 규칙을 작성하세요 -->
        """;

    private const string ClaudeSettingsTemplate =
        """
        {
          "permissions": {
            "allow": [],
            "deny": []
          },
          "hooks": {
            "PreToolUse": [],
            "PostToolUse": [],
            "Notification": []
          }
        }
        """;

    private const string ClaudeSettingsLocalTemplate =
        """
        {
          "permissions": {
            "allow": [],
            "deny": []
          }
        }
        """;

    private const string ClaudeRulesTemplate =
        """
        ---
        description: <!-- 이 규칙이 적용되는 상황을 설명하세요 -->
        globs:
        alwaysApply: false
        ---

        # Rule Name

        ## Guidelines
        <!-- 규칙 내용을 작성하세요 -->
        """;

    private const string AgentDefinitionTemplate =
        """
        ---
        name: <!-- 에이전트 이름 -->
        description: <!-- 에이전트 설명 -->
        ---

        # Agent Instructions

        ## Role
        <!-- 에이전트의 역할을 정의하세요 -->

        ## Tools
        <!-- 사용 가능한 도구를 나열하세요 -->

        ## Constraints
        <!-- 제약 조건을 명시하세요 -->
        """;

    private const string McpConfigTemplate =
        """
        {
          "mcpServers": {
            "example-server": {
              "command": "npx",
              "args": ["-y", "@example/mcp-server"],
              "env": {}
            }
          }
        }
        """;

    private const string AgentsMdTemplate =
        """
        # Agents

        ## Agent 1
        <!-- 에이전트 역할과 책임을 정의하세요 -->

        ### Responsibilities
        -

        ### Tools
        -
        """;

    private const string EnvConfigTemplate =
        """
        # Environment Variables
        # 이 파일에는 민감한 정보가 포함될 수 있으므로 Git에 커밋하지 마세요

        # API_KEY=your-api-key-here
        # DATABASE_URL=postgresql://localhost:5432/mydb
        """;

    private const string MemoryTemplate =
        """
        # Memory

        <!-- 에이전트 자동 메모리 파일입니다 -->
        <!-- 프로젝트에서 학습한 컨텍스트가 여기에 저장됩니다 -->
        """;
}
