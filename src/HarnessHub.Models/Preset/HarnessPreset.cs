using HarnessHub.Models.Harness;

namespace HarnessHub.Models.Preset;

/// <summary>
/// 하네스 프리셋을 나타내는 도메인 모델.
/// 프리셋 폴더에 저장된 하네스 파일 묶음의 메타데이터.
/// </summary>
public sealed record HarnessPreset
{
    /// <summary>프리셋 이름.</summary>
    public required string Name { get; init; }

    /// <summary>적용 범위 (Global/Project).</summary>
    public required HarnessScope Scope { get; init; }

    /// <summary>프리셋 설명.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>포함된 파일 목록.</summary>
    public required IReadOnlyList<PresetFileEntry> Files { get; init; }

    /// <summary>총 토큰 수.</summary>
    public int TotalTokens { get; init; }

    /// <summary>생성 일시.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>최종 수정 일시.</summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>프리셋 폴더의 절대 경로.</summary>
    public required string FolderPath { get; init; }
}
