using FluentAssertions;
using HarnessHub.Infrastructure.Settings;
using HarnessHub.Models.Harness;

namespace HarnessHub.Tests.Infrastructure;

/// <summary>
/// AppSettingsService의 설정 로드/저장/이벤트 테스트.
/// </summary>
public class AppSettingsServiceTests
{
    [Fact]
    public void Should_Have_Default_Provider_ClaudeCode()
    {
        var service = new AppSettingsService();
        service.ActiveProvider.Should().Be(HarnessProvider.ClaudeCode);
    }

    [Fact]
    public void Should_Have_Default_ContextWindowSize()
    {
        var service = new AppSettingsService();
        service.ContextWindowSize.Should().Be(200_000);
    }

    [Fact]
    public void SetProvider_Should_Update_ActiveProvider()
    {
        var service = new AppSettingsService();

        service.SetProvider(HarnessProvider.Cursor);

        service.ActiveProvider.Should().Be(HarnessProvider.Cursor);

        // 원래값으로 복원
        service.SetProvider(HarnessProvider.ClaudeCode);
    }

    [Fact]
    public void SetProvider_Should_Fire_ProviderChanged_Event()
    {
        var service = new AppSettingsService();
        HarnessProvider? received = null;
        service.ProviderChanged += p => received = p;

        service.SetProvider(HarnessProvider.Cursor);

        received.Should().Be(HarnessProvider.Cursor);

        // 복원
        service.SetProvider(HarnessProvider.ClaudeCode);
    }

    [Fact]
    public void SetProvider_Should_Not_Fire_Event_If_Same_Value()
    {
        var service = new AppSettingsService();
        var eventFired = false;
        service.ProviderChanged += _ => eventFired = true;

        service.SetProvider(service.ActiveProvider);

        eventFired.Should().BeFalse();
    }

    [Fact]
    public void SetContextWindowSize_Should_Update_Value()
    {
        var service = new AppSettingsService();

        service.SetContextWindowSize(1_000_000);

        service.ContextWindowSize.Should().Be(1_000_000);

        // 복원
        service.SetContextWindowSize(200_000);
    }
}
