namespace HarnessHub.Abstract.Services;

/// <summary>
/// 프로세스 전역에서 공유되는 프로젝트 컨텍스트.
/// 현재 프로젝트 경로와 글로벌 경로를 보유하며, 변경 시 메시지를 발행한다.
/// </summary>
public interface IProjectContext
{
    /// <summary>
    /// 글로벌 하네스 경로 (~/.claude).
    /// </summary>
    string GlobalPath { get; }

    /// <summary>
    /// 현재 프로젝트 경로. null이면 프로젝트 미선택 상태.
    /// </summary>
    string? ProjectPath { get; }

    /// <summary>
    /// 프로젝트 경로를 변경한다.
    /// 변경 시 ProjectPathChangedMessage를 발행한다.
    /// </summary>
    /// <param name="path">새 프로젝트 경로.</param>
    void SetProjectPath(string path);
}
