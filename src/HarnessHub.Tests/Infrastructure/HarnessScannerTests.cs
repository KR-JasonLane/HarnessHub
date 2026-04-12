using FluentAssertions;
using HarnessHub.Abstract.Services;
using HarnessHub.Infrastructure.Harness;
using HarnessHub.Models.Harness;
using Moq;

namespace HarnessHub.Tests.Infrastructure;

/// <summary>
/// HarnessScanner의 스캔 및 파일 감지 로직 테스트.
/// </summary>
public class HarnessScannerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly HarnessScanner _scanner;

    public HarnessScannerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "HarnessHub_Test_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);

        var tokenCounter = new Mock<ITokenCounterService>();
        tokenCounter.Setup(t => t.CountTokens(It.IsAny<string>())).Returns(10);

        var appSettings = new Mock<IAppSettingsService>();
        appSettings.Setup(s => s.ActiveProvider).Returns(HarnessProvider.ClaudeCode);

        _scanner = new HarnessScanner(tokenCounter.Object, appSettings.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task ScanAsync_Should_Return_Empty_For_NonExistent_Folder()
    {
        var result = await _scanner.ScanAsync("/nonexistent/path", HarnessScope.Project);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ScanAsync_Should_Detect_ClaudeMd_In_Project()
    {
        File.WriteAllText(Path.Combine(_tempDir, "CLAUDE.md"), "# Test");

        var result = await _scanner.ScanAsync(_tempDir, HarnessScope.Project);

        result.Should().ContainSingle(f => f.FileType == HarnessFileType.ClaudeMd);
    }

    [Fact]
    public async Task ScanAsync_Should_Detect_Multiple_Files()
    {
        File.WriteAllText(Path.Combine(_tempDir, "CLAUDE.md"), "# Test");
        File.WriteAllText(Path.Combine(_tempDir, "AGENTS.md"), "# Agents");
        File.WriteAllText(Path.Combine(_tempDir, ".env"), "KEY=VALUE");

        var result = await _scanner.ScanAsync(_tempDir, HarnessScope.Project);

        result.Should().HaveCountGreaterThanOrEqualTo(3);
        result.Should().Contain(f => f.FileType == HarnessFileType.ClaudeMd);
        result.Should().Contain(f => f.FileType == HarnessFileType.AgentsMd);
        result.Should().Contain(f => f.FileType == HarnessFileType.EnvConfig);
    }

    [Fact]
    public async Task ScanAsync_Should_Detect_ClaudeSettings_In_Subdirectory()
    {
        var claudeDir = Path.Combine(_tempDir, ".claude");
        Directory.CreateDirectory(claudeDir);
        File.WriteAllText(Path.Combine(claudeDir, "settings.json"), "{}");

        var result = await _scanner.ScanAsync(_tempDir, HarnessScope.Project);

        result.Should().Contain(f => f.FileType == HarnessFileType.ClaudeSettings);
    }

    [Fact]
    public async Task ScanAsync_Should_Scan_Rules_Directory()
    {
        var rulesDir = Path.Combine(_tempDir, ".claude", "rules");
        Directory.CreateDirectory(rulesDir);
        File.WriteAllText(Path.Combine(rulesDir, "mvvm.md"), "# MVVM Rules");
        File.WriteAllText(Path.Combine(rulesDir, "naming.md"), "# Naming Rules");

        var result = await _scanner.ScanAsync(_tempDir, HarnessScope.Project);

        result.Where(f => f.FileType == HarnessFileType.ClaudeRules).Should().HaveCount(2);
    }

    [Fact]
    public void GetCreatableFiles_Should_Exclude_Existing_Files()
    {
        File.WriteAllText(Path.Combine(_tempDir, "CLAUDE.md"), "# Test");

        var existing = new List<HarnessFileInfo>
        {
            new()
            {
                FilePath = Path.Combine(_tempDir, "CLAUDE.md"),
                FileName = "CLAUDE.md",
                FileType = HarnessFileType.ClaudeMd,
                Scope = HarnessScope.Project,
                Lever = HarnessLever.SystemPrompt
            }
        };

        var creatable = _scanner.GetCreatableFiles(_tempDir, HarnessScope.Project, existing);

        creatable.Should().NotContain(f => f.RelativePath == "CLAUDE.md");
    }

    [Fact]
    public void GetCreatableFiles_Should_Include_Wildcard_Directories()
    {
        var creatable = _scanner.GetCreatableFiles(_tempDir, HarnessScope.Project, new List<HarnessFileInfo>());

        creatable.Should().Contain(f => f.IsDirectory && f.RelativePath.Contains("rules"));
        creatable.Should().Contain(f => f.IsDirectory && f.RelativePath.Contains("agents"));
    }
}
