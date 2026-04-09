namespace HarnessHub.Models.Preset;

/// <summary>
/// 프리셋 내보내기/가져오기용 번들.
/// 메타데이터와 파일 내용을 단일 JSON으로 직렬화한다.
/// </summary>
public sealed record PresetBundle
{
    /// <summary>프리셋 메타데이터.</summary>
    public required PresetMetadata Metadata { get; init; }

    /// <summary>파일 내용 딕셔너리. Key: RelativePath, Value: 파일 내용 텍스트.</summary>
    public required IReadOnlyDictionary<string, string> FileContents { get; init; }

    /// <summary>내보내기 일시.</summary>
    public DateTime ExportedAt { get; init; }
}
