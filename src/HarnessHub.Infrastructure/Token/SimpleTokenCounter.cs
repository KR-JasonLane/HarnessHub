using HarnessHub.Abstract.Services;

namespace HarnessHub.Infrastructure.Token;

/// <summary>
/// 단순 추정 기반 토큰 카운터. 단어 수 * 1.3으로 추정한다.
/// Phase 6에서 SharpToken으로 교체 예정.
/// </summary>
public sealed class SimpleTokenCounter : ITokenCounterService
{
    public int CountTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var charCount = text.Length;
        return (int)(charCount / 4.0 + 0.5);
    }
}
