using HarnessHub.Models.Harness;

namespace HarnessHub.Models.Explorer;

/// <summary>
/// 폴더 트리의 노드를 나타낸다. 디렉토리 또는 파일 모두 표현할 수 있다.
/// </summary>
public sealed class FolderNode
{
    public required string Name { get; init; }
    public required string FullPath { get; init; }
    public required bool IsDirectory { get; init; }
    public bool IsHarnessFile { get; init; }
    public HarnessFileType? HarnessFileType { get; init; }
    public bool IsExpanded { get; set; }
    public List<FolderNode> Children { get; init; } = new();
}
