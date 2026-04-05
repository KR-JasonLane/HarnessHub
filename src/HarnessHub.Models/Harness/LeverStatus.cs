namespace HarnessHub.Models.Harness;

/// <summary>
/// 하네스 레버의 활성 상태를 나타낸다.
/// </summary>
public sealed record LeverStatus
{
    public required HarnessLever Lever { get; init; }
    public required bool IsActive { get; init; }
    public required int FileCount { get; init; }
    public required string Description { get; init; }
}
