namespace HarnessHub.Abstract.Services;

/// <summary>
/// Light/Dark 테마 전환 서비스 인터페이스.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// 현재 다크 모드 여부.
    /// </summary>
    bool IsDarkTheme { get; }

    /// <summary>
    /// 테마를 토글한다 (Light ↔ Dark).
    /// </summary>
    void ToggleTheme();

    /// <summary>
    /// 지정된 테마로 변경한다.
    /// </summary>
    /// <param name="isDark">true이면 Dark, false이면 Light.</param>
    void SetTheme(bool isDark);
}
