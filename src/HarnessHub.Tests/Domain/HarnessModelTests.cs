using FluentAssertions;
using HarnessHub.Models.Harness;
using HarnessHub.Models.Preset;

namespace HarnessHub.Tests.Domain;

/// <summary>
/// 도메인 모델 record 생성 및 불변성 테스트.
/// </summary>
public class HarnessModelTests
{
    [Fact]
    public void HarnessFileInfo_Should_Create_With_Required_Properties()
    {
        var info = new HarnessFileInfo
        {
            FilePath = "/test/CLAUDE.md",
            FileName = "CLAUDE.md",
            FileType = HarnessFileType.ClaudeMd,
            Scope = HarnessScope.Project,
            Lever = HarnessLever.SystemPrompt,
            TokenCount = 100,
            LastModified = DateTime.UtcNow
        };

        info.FilePath.Should().Be("/test/CLAUDE.md");
        info.FileType.Should().Be(HarnessFileType.ClaudeMd);
        info.TokenCount.Should().Be(100);
    }

    [Fact]
    public void HarnessPreset_Should_Create_With_Required_Properties()
    {
        var files = new List<PresetFileEntry>
        {
            new() { RelativePath = "CLAUDE.md", FileType = HarnessFileType.ClaudeMd, TokenCount = 50 }
        };

        var preset = new HarnessPreset
        {
            Name = "테스트 프리셋",
            Scope = HarnessScope.Global,
            Description = "테스트용",
            Files = files,
            TotalTokens = 50,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            FolderPath = "/presets/test"
        };

        preset.Name.Should().Be("테스트 프리셋");
        preset.Files.Should().HaveCount(1);
        preset.TotalTokens.Should().Be(50);
    }

    [Fact]
    public void LeverStatus_Should_Create_With_Properties()
    {
        var status = new LeverStatus
        {
            Lever = HarnessLever.Hook,
            IsActive = true,
            FileCount = 3,
            Description = "Hooks"
        };

        status.IsActive.Should().BeTrue();
        status.FileCount.Should().Be(3);
    }

    [Fact]
    public void CreatableHarnessFile_Should_Have_Default_IsDirectory_False()
    {
        var file = new CreatableHarnessFile
        {
            RelativePath = "CLAUDE.md",
            FileType = HarnessFileType.ClaudeMd,
            Lever = HarnessLever.SystemPrompt,
            Scope = HarnessScope.Project,
            Description = "Claude 시스템 프롬프트"
        };

        file.IsDirectory.Should().BeFalse();
        file.FileExtension.Should().BeNull();
    }
}
