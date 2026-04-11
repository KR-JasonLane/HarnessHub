using System.IO;
using HarnessHub.Abstract.Services;

namespace HarnessHub.Infrastructure.Project;

/// <summary>
/// 프로세스 전역에서 공유되는 프로젝트 컨텍스트 구현.
/// </summary>
public sealed class ProjectContext : IProjectContext
{
    public string GlobalPath { get; }

    public string? ProjectPath { get; private set; }

    /// <inheritdoc />
    public event Action<string>? ProjectPathChanged;

    public ProjectContext()
    {
        GlobalPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".claude");
    }

    public void SetProjectPath(string path)
    {
        ProjectPath = path;
        ProjectPathChanged?.Invoke(path);
    }
}
