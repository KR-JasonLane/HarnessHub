namespace HarnessHub.Models.Preset;

/// <summary>
/// preset.json 내 파일 항목의 직렬화용 DTO.
/// </summary>
public sealed record PresetFileEntryDto
{
    /// <summary>프리셋 폴더 기준 상대 경로.</summary>
    public required string RelativePath { get; init; }

    /// <summary>하네스 파일 유형 문자열.</summary>
    public required string FileType { get; init; }

    /// <summary>토큰 수.</summary>
    public int TokenCount { get; init; }
}
