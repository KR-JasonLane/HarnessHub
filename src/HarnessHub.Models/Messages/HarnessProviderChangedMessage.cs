using HarnessHub.Models.Harness;

namespace HarnessHub.Models.Messages;

/// <summary>
/// 하네스 프로바���더가 변경되었음을 알리는 ���시지.
/// AppSettingsService에서 발행, Dashboard/Explorer 등에서 수신하여 새로고침한다.
/// </summary>
public sealed record HarnessProviderChangedMessage(HarnessProvider Provider);
