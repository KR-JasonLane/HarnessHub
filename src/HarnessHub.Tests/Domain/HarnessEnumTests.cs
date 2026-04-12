using FluentAssertions;
using HarnessHub.Models.Harness;

namespace HarnessHub.Tests.Domain;

/// <summary>
/// 하네스 도메인 열거형 값 검증 테스트.
/// </summary>
public class HarnessEnumTests
{
    [Fact]
    public void HarnessFileType_Should_Have_Expected_Values()
    {
        var values = Enum.GetValues<HarnessFileType>();
        values.Should().Contain(HarnessFileType.ClaudeMd);
        values.Should().Contain(HarnessFileType.ClaudeSettings);
        values.Should().Contain(HarnessFileType.ClaudeRules);
        values.Should().Contain(HarnessFileType.McpConfig);
        values.Should().Contain(HarnessFileType.AgentDefinition);
        values.Should().Contain(HarnessFileType.AgentsMd);
        values.Should().Contain(HarnessFileType.EnvConfig);
    }

    [Fact]
    public void HarnessProvider_Should_Have_ClaudeCode_And_Cursor()
    {
        var values = Enum.GetValues<HarnessProvider>();
        values.Should().HaveCount(2);
        values.Should().Contain(HarnessProvider.ClaudeCode);
        values.Should().Contain(HarnessProvider.Cursor);
    }

    [Fact]
    public void HarnessScope_Should_Have_Global_And_Project()
    {
        var values = Enum.GetValues<HarnessScope>();
        values.Should().HaveCount(2);
        values.Should().Contain(HarnessScope.Global);
        values.Should().Contain(HarnessScope.Project);
    }

    [Fact]
    public void HarnessLever_Should_Have_Five_Levers()
    {
        var values = Enum.GetValues<HarnessLever>();
        values.Should().HaveCount(5);
        values.Should().Contain(HarnessLever.SystemPrompt);
        values.Should().Contain(HarnessLever.Skill);
        values.Should().Contain(HarnessLever.McpServer);
        values.Should().Contain(HarnessLever.SubAgent);
        values.Should().Contain(HarnessLever.Hook);
    }
}
