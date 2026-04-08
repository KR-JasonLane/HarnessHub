using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using HarnessHub.Abstract.Services;
using HarnessHub.Models.Messages;

namespace HarnessHub.Infrastructure.Project;

/// <summary>
/// 프로세스 전역에서 공유되는 프로젝트 컨텍스트 구현.
/// 프로젝트 경로 변경 시 WeakReferenceMessenger를 통해 알린다.
/// </summary>
public sealed class ProjectContext : IProjectContext
{
    public string GlobalPath { get; }

    public string? ProjectPath { get; private set; }

    public ProjectContext()
    {
        GlobalPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".claude");
    }

    public void SetProjectPath(string path)
    {
        ProjectPath = path;
        WeakReferenceMessenger.Default.Send(new ProjectPathChangedMessage(path));
    }
}
