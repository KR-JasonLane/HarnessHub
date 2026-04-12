using FluentAssertions;
using HarnessHub.Infrastructure.Template;
using HarnessHub.Models.Harness;

namespace HarnessHub.Tests.Infrastructure;

/// <summary>
/// HarnessTemplateService의 템플릿 콘텐츠 반환 테스트.
/// </summary>
public class HarnessTemplateServiceTests
{
    private readonly HarnessTemplateService _service = new();

    [Theory]
    [InlineData(HarnessFileType.ClaudeMd)]
    [InlineData(HarnessFileType.ClaudeLocalMd)]
    [InlineData(HarnessFileType.ClaudeRules)]
    [InlineData(HarnessFileType.AgentDefinition)]
    [InlineData(HarnessFileType.AgentsMd)]
    [InlineData(HarnessFileType.EnvConfig)]
    [InlineData(HarnessFileType.Memory)]
    public void GetTemplate_Should_Return_NonEmpty_For_Markdown_Types(HarnessFileType fileType)
    {
        var template = _service.GetTemplate(fileType);
        template.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(HarnessFileType.ClaudeSettings)]
    [InlineData(HarnessFileType.ClaudeSettingsLocal)]
    [InlineData(HarnessFileType.McpConfig)]
    public void GetTemplate_Should_Return_Valid_Json_For_Config_Types(HarnessFileType fileType)
    {
        var template = _service.GetTemplate(fileType);
        template.Should().NotBeNullOrWhiteSpace();
        template.Should().Contain("{");
        template.Should().Contain("}");
    }

    [Fact]
    public void GetTemplate_ClaudeMd_Should_Contain_Section_Headers()
    {
        var template = _service.GetTemplate(HarnessFileType.ClaudeMd);
        template.Should().Contain("# ");
        template.Should().Contain("## ");
    }

    [Fact]
    public void GetTemplate_McpConfig_Should_Contain_McpServers_Key()
    {
        var template = _service.GetTemplate(HarnessFileType.McpConfig);
        template.Should().Contain("mcpServers");
    }

    [Fact]
    public void GetTemplate_ClaudeSettings_Should_Contain_Permissions_And_Hooks()
    {
        var template = _service.GetTemplate(HarnessFileType.ClaudeSettings);
        template.Should().Contain("permissions");
        template.Should().Contain("hooks");
    }
}
