using HarnessHub.Models.Harness;
using HarnessHub.Models.Preset;

namespace HarnessHub.Abstract.Services;

/// <summary>
/// 프리셋 CRUD, 적용, 내보내기/가져오기를 담당한다.
/// </summary>
public interface IPresetService
{
    /// <summary>
    /// 지정된 범위의 모든 프리셋을 로드한다.
    /// </summary>
    /// <param name="scope">글로벌 또는 프로젝트 범위.</param>
    /// <returns>프리셋 목록.</returns>
    Task<IReadOnlyList<HarnessPreset>> LoadAllAsync(HarnessScope scope);

    /// <summary>
    /// 프리셋을 저장한다. 기존 프리셋이면 덮어쓴다.
    /// </summary>
    /// <param name="preset">저장할 프리셋 메타데이터.</param>
    /// <param name="sourceFilePaths">프리셋에 포함할 원본 파일의 절대 경로 목록.</param>
    Task SaveAsync(HarnessPreset preset, IReadOnlyList<string> sourceFilePaths);

    /// <summary>
    /// 프리셋을 삭제한다.
    /// </summary>
    /// <param name="preset">삭제할 프리셋.</param>
    Task DeleteAsync(HarnessPreset preset);

    /// <summary>
    /// 프리셋을 적용한다. 프리셋 파일을 대상 경로에 복사한다.
    /// </summary>
    /// <param name="preset">적용할 프리셋.</param>
    /// <param name="targetPath">적용 대상 경로 (GlobalPath 또는 ProjectPath).</param>
    /// <param name="backup">기존 파일 백업 여부.</param>
    Task ApplyAsync(HarnessPreset preset, string targetPath, bool backup = true);

    /// <summary>
    /// 프리셋을 단일 JSON 번들 파일로 내보낸다.
    /// </summary>
    /// <param name="preset">내보낼 프리셋.</param>
    /// <param name="exportPath">내보낼 JSON 파일 경로.</param>
    Task ExportAsync(HarnessPreset preset, string exportPath);

    /// <summary>
    /// JSON 번들 파일에서 프리셋을 가져온다.
    /// </summary>
    /// <param name="jsonBundlePath">가져올 JSON 번들 파일 경로.</param>
    /// <param name="scope">가져올 범위.</param>
    /// <returns>가져온 프리셋.</returns>
    Task<HarnessPreset> ImportAsync(string jsonBundlePath, HarnessScope scope);

    /// <summary>
    /// 현재 하네스 파일로부터 새 프리셋을 캡처한다.
    /// 지정된 경로를 스캔하여 발견된 하네스 파일을 프리셋 폴더에 복사한다.
    /// </summary>
    /// <param name="name">프리셋 이름.</param>
    /// <param name="description">프리셋 설명.</param>
    /// <param name="sourcePath">스캔할 소스 경로.</param>
    /// <param name="scope">프리셋 범위.</param>
    /// <returns>생성된 프리셋.</returns>
    Task<HarnessPreset> CaptureCurrentAsync(string name, string description, string sourcePath, HarnessScope scope);

    /// <summary>
    /// 현재 활성 프리셋 상태를 로드한다.
    /// </summary>
    /// <returns>활성 프리셋 상태.</returns>
    Task<ActivePresetState> GetActiveStateAsync();

    /// <summary>
    /// 활성 프리셋 상태를 저장한다.
    /// </summary>
    /// <param name="state">저장할 활성 상태.</param>
    Task SetActiveStateAsync(ActivePresetState state);
}
