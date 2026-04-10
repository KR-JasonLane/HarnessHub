namespace HarnessHub.Infrastructure.Settings;

/// <summary>
/// 앱 설정 JSON 직렬화용 DTO.
/// %AppData%/HarnessHub/settings.json에 저장된다.
/// </summary>
internal sealed class AppSettingsDto
{
    public string Provider { get; set; } = "ClaudeCode";
    public int ContextWindowSize { get; set; } = 200_000;
}
