namespace HarnessHub.Models.Harness;

/// <summary>
/// 하네스 파일의 메타데이터를 나타낸다.
/// </summary>
public sealed record HarnessFileInfo
{
    public required string FilePath { get; init; }
    public required string FileName { get; init; }
    public required HarnessFileType FileType { get; init; }
    public required HarnessScope Scope { get; init; }
    public required HarnessLever Lever { get; init; }
    public int TokenCount { get; init; }
    public DateTime LastModified { get; init; }
}
