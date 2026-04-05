using HarnessHub.Abstract.ViewModels;

namespace HarnessHub.Abstract.Services;

/// <summary>
/// NavigationRail 인덱스에 따라 콘텐츠 ViewModel을 생성한다.
/// App 레이어에서 구현하여 모듈 간 직접 참조를 방지한다.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// 네비게이션 인덱스에 해당하는 콘텐츠 ViewModel을 반환한다.
    /// </summary>
    /// <param name="index">NavigationRail 인덱스 (0: Dashboard, 1: Explorer, 2: Editor, 3: Preset).</param>
    /// <returns>콘텐츠 ViewModel, 또는 미구현 시 null.</returns>
    IContentViewModel? ResolveContent(int index);
}
