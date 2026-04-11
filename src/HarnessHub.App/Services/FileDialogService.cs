using HarnessHub.Abstract.Services;
using Microsoft.Win32;

namespace HarnessHub.App.Services;

/// <summary>
/// Win32 파일/폴더 다이얼로그 구현.
/// </summary>
public sealed class FileDialogService : IFileDialogService
{
    /// <inheritdoc />
    public string? ShowOpenFolderDialog(string title)
    {
        var dialog = new OpenFolderDialog { Title = title };
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    /// <inheritdoc />
    public string? ShowOpenFileDialog(string title, string filter)
    {
        var dialog = new OpenFileDialog { Title = title, Filter = filter };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    /// <inheritdoc />
    public string? ShowSaveFileDialog(string title, string fileName, string filter)
    {
        var dialog = new SaveFileDialog { Title = title, FileName = fileName, Filter = filter };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
