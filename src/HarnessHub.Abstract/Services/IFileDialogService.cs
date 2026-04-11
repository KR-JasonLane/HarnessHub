namespace HarnessHub.Abstract.Services;

/// <summary>
/// 파일/폴더 다이얼로그 추상화.
/// ViewModel이 Win32 다이얼로그에 직접 의존하지 않도록 한다.
/// </summary>
public interface IFileDialogService
{
    /// <summary>폴더 선택 다이얼로그를 표시한다.</summary>
    string? ShowOpenFolderDialog(string title);

    /// <summary>파일 열기 다이얼로그를 표시한다.</summary>
    string? ShowOpenFileDialog(string title, string filter);

    /// <summary>파일 저장 다이얼로그를 표시한다.</summary>
    string? ShowSaveFileDialog(string title, string fileName, string filter);
}
