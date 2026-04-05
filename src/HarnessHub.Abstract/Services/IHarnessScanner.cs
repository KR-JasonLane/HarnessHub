using HarnessHub.Models.Harness;

namespace HarnessHub.Abstract.Services;

/// <summary>
/// 폴더를 스캔하여 하네스 파일 목록을 반환한다.
/// </summary>
public interface IHarnessScanner
{
    /// <summary>
    /// 지정된 폴더에서 하네스 파일을 스캔한다.
    /// </summary>
    /// <param name="folderPath">스캔할 폴더 경로.</param>
    /// <param name="scope">글로벌 또는 프로젝트 범위.</param>
    /// <returns>감지된 하네스 파일 목록.</returns>
    Task<IReadOnlyList<HarnessFileInfo>> ScanAsync(string folderPath, HarnessScope scope);
}
