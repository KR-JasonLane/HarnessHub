namespace HarnessHub.Models.Messages;

/// <summary>
/// 하네스 파일을 에디터에서 열도록 요청하는 메시지.
/// Dashboard/Explorer에서 발행, ShellWindowViewModel에서 수신한다.
/// </summary>
public sealed record OpenFileMessage(string FilePath);
