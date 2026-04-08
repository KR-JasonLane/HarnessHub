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

    /// <summary>
    /// 아직 생성되지 않은, 생성 가능한 하네스 파일 목록을 반환한다.
    /// </summary>
    /// <param name="folderPath">기준 폴더 경로.</param>
    /// <param name="scope">글로벌 또는 프로젝트 범위.</param>
    /// <param name="existingFiles">이미 존재하는 하네스 파일 목록.</param>
    /// <returns>생성 가능한 하네스 파일 목록.</returns>
    IReadOnlyList<CreatableHarnessFile> GetCreatableFiles(
        string folderPath,
        HarnessScope scope,
        IReadOnlyList<HarnessFileInfo> existingFiles);
}
