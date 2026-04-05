namespace HarnessHub.Models.Harness;

/// <summary>
/// 하네스 구성 현황 요약을 나타낸다.
/// </summary>
public sealed record HarnessSummary
{
    public string? ProjectPath { get; init; }
    public required string GlobalPath { get; init; }
    public required IReadOnlyList<LeverStatus> LeverStatuses { get; init; }
    public required IReadOnlyList<HarnessFileInfo> Files { get; init; }
    public int TotalTokens { get; init; }
    public int ContextWindowSize { get; init; } = 200_000;
}
