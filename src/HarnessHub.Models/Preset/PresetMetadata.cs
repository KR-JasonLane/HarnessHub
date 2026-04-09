namespace HarnessHub.Models.Preset;

/// <summary>
/// preset.json 직렬화용 DTO. 프리셋의 메타데이터를 나타낸다.
/// </summary>
public sealed record PresetMetadata
{
    /// <summary>프리셋 이름.</summary>
    public required string Name { get; init; }

    /// <summary>적용 범위 (Global/Project).</summary>
    public required string Scope { get; init; }

    /// <summary>프리셋 설명.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>포함된 파일 목록.</summary>
    public required IReadOnlyList<PresetFileEntryDto> Files { get; init; }

    /// <summary>총 토큰 수.</summary>
    public int TotalTokens { get; init; }

    /// <summary>생성 일시.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>최종 수정 일시.</summary>
    public DateTime UpdatedAt { get; init; }
}
