namespace HarnessHub.Models.Messages;

/// <summary>
/// 프로젝트 경로가 변경되었음을 알리는 메시지.
/// IProjectContext에서 발행, 각 ViewModel에서 수신한다.
/// </summary>
public sealed record ProjectPathChangedMessage(string ProjectPath);
