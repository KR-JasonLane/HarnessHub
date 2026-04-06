using HarnessHub.Models.Explorer;
using HarnessHub.Models.Harness;

namespace HarnessHub.Abstract.Services;

/// <summary>
/// 폴더 구조를 스캔하여 트리 노드를 생성한다.
/// </summary>
public interface IFileExplorerService
{
    /// <summary>
    /// 루트 경로를 기준으로 폴더 트리를 구성한다.
    /// 하네스 파일은 하이라이트 표시를 위해 마킹한다.
    /// </summary>
    /// <param name="rootPath">스캔할 루트 폴더 경로.</param>
    /// <param name="harnessFiles">감지된 하네스 파일 목록.</param>
    /// <returns>트리 루트 노드.</returns>
    Task<FolderNode> BuildFolderTreeAsync(string rootPath, IReadOnlyList<HarnessFileInfo> harnessFiles);
}
