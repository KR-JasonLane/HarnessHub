using System.IO;
using System.Text.Json;
using HarnessHub.Abstract.Services;
using MaterialDesignThemes.Wpf;
using Serilog;

namespace HarnessHub.App.Services;

/// <summary>
/// MaterialDesign PaletteHelper를 이용한 Light/Dark 테마 전환 서비스.
/// 테마 설정을 %AppData%/HarnessHub/theme.json에 저장하여 앱 재시작 시에도 유지한다.
/// </summary>
public class ThemeService : IThemeService
{
    private readonly PaletteHelper _paletteHelper = new();
    private readonly string _settingsPath;

    public ThemeService()
    {
        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HarnessHub");

        Directory.CreateDirectory(appDataDir);
        _settingsPath = Path.Combine(appDataDir, "theme.json");

        LoadAndApply();
    }

    public bool IsDarkTheme
    {
        get
        {
            var theme = _paletteHelper.GetTheme();
            return theme.GetBaseTheme() == BaseTheme.Dark;
        }
    }

    public void ToggleTheme()
    {
        SetTheme(!IsDarkTheme);
    }

    public void SetTheme(bool isDark)
    {
        var theme = _paletteHelper.GetTheme();
        theme.SetBaseTheme(isDark ? BaseTheme.Dark : BaseTheme.Light);
        _paletteHelper.SetTheme(theme);
        Save(isDark);
    }

    private void LoadAndApply()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return;
            }

            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<ThemeSettings>(json);
            if (settings is null)
            {
                return;
            }

            var theme = _paletteHelper.GetTheme();
            theme.SetBaseTheme(settings.IsDark ? BaseTheme.Dark : BaseTheme.Light);
            _paletteHelper.SetTheme(theme);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load theme settings");
        }
    }

    private void Save(bool isDark)
    {
        try
        {
            var json = JsonSerializer.Serialize(new ThemeSettings { IsDark = isDark });
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to save theme settings");
        }
    }
}
