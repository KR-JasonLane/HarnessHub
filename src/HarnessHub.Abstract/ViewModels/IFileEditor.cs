namespace HarnessHub.Abstract.ViewModels;

/// <summary>
/// 파일을 편집할 수 있는 ViewModel이 구현하는 인터페이스.
/// Shell에서 OpenFileMessage 수신 시 에디터에 파일을 전달하는 데 사용된다.
/// </summary>
public interface IFileEditor
{
    /// <summary>
    /// 지정된 경로의 파일을 에디터에 로드한다.
    /// </summary>
    /// <param name="path">로드할 파일의 전체 경로.</param>
    Task LoadFileAsync(string path);
}
