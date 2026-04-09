using HarnessHub.Models.Harness;

namespace HarnessHub.Models.Preset;

/// <summary>
/// 프리셋에 포함된 개별 하네스 파일의 메타데이터.
/// </summary>
public sealed record PresetFileEntry
{
    /// <summary>프리셋 폴더 기준 상대 경로.</summary>
    public required string RelativePath { get; init; }

    /// <summary>하네스 파일 유형.</summary>
    public required HarnessFileType FileType { get; init; }

    /// <summary>토큰 수.</summary>
    public int TokenCount { get; init; }
}
