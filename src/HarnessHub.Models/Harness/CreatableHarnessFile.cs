namespace HarnessHub.Models.Harness;

/// <summary>
/// 아직 생성되지 않은, 생성 가능한 하네스 파일 정보.
/// 에디터 홈 화면에서 "새 파일 추가" 목록에 표시된다.
/// </summary>
public sealed record CreatableHarnessFile
{
    /// <summary>
    /// 기준 폴더로부터의 상대 경로 (예: "CLAUDE.md", ".claude/rules/").
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// 파일 유형.
    /// </summary>
    public required HarnessFileType FileType { get; init; }

    /// <summary>
    /// 소속 레버.
    /// </summary>
    public required HarnessLever Lever { get; init; }

    /// <summary>
    /// 범위 (Global / Project).
    /// </summary>
    public required HarnessScope Scope { get; init; }

    /// <summary>
    /// 사용자에게 표시할 설명.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// 와일드카드 디렉토리 여부 (rules/, agents/ 등).
    /// true이면 사용자에게 파일명 입력을 요청한다.
    /// </summary>
    public bool IsDirectory { get; init; }

    /// <summary>
    /// 와일드카드 디렉토리의 파일 확장자 (예: ".md", ".mdc").
    /// </summary>
    public string? FileExtension { get; init; }
}
