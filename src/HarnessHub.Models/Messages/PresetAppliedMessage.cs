using HarnessHub.Models.Harness;

namespace HarnessHub.Models.Messages;

/// <summary>
/// 프리셋이 적용되었음을 알리는 메시지.
/// PresetViewModel에서 발행, DashboardViewModel에서 수신하여 새로고침한다.
/// </summary>
public sealed record PresetAppliedMessage(string PresetName, HarnessScope Scope);
