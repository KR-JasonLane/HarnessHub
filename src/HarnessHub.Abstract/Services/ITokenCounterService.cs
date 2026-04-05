namespace HarnessHub.Abstract.Services;

/// <summary>
/// 텍스트의 토큰 수를 계산한다.
/// </summary>
public interface ITokenCounterService
{
    /// <summary>
    /// 텍스트의 토큰 수를 추정한다.
    /// </summary>
    /// <param name="text">토큰 수를 계산할 텍스트.</param>
    /// <returns>추정 토큰 수.</returns>
    int CountTokens(string text);
}
