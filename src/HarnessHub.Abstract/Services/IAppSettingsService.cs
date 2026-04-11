using HarnessHub.Models.Harness;

namespace HarnessHub.Abstract.Services;

/// <summary>
/// 앱 전역 설정 ���비스.
/// 프로바이더 선택, 컨텍스트 윈도우 크기 등 사용자 ���정을 관리한다.
/// </summary>
public interface IAppSettingsService
{
    /// <summary>
    /// 현재 활성 하네스 ���로바이더.
    /// </summary>
    HarnessProvider ActiveProvider { get; }

    /// <summary>
    /// 컨텍스트 윈도우 크기 (토큰 단위).
    /// </summary>
    int ContextWindowSize { get; }

    /// <summary>
    /// 하네스 프로바이더가 변경되었을 때 발생한다.
    /// </summary>
    event Action<HarnessProvider>? ProviderChanged;

    /// <summary>
    /// 하네스 프로바이더를 변경한다.
    /// </summary>
    void SetProvider(HarnessProvider provider);

    /// <summary>
    /// ���텍스트 윈도우 크기를 변경한다.
    /// </summary>
    void SetContextWindowSize(int size);
}
