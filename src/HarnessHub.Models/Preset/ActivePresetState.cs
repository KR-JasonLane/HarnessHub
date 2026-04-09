namespace HarnessHub.Models.Preset;

/// <summary>
/// 현재 활성화된 프리셋 상태. active.json 직렬화용 DTO.
/// </summary>
public sealed record ActivePresetState
{
    /// <summary>현재 적용된 글로벌 프리셋 이름.</summary>
    public string? GlobalPresetName { get; init; }

    /// <summary>현재 적용된 프로젝트 프리셋 이름.</summary>
    public string? ProjectPresetName { get; init; }
}
