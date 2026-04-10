using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using HarnessHub.Abstract.Services;
using HarnessHub.Models.Harness;
using HarnessHub.Models.Messages;
using Serilog;

namespace HarnessHub.Infrastructure.Settings;

/// <summary>
/// 앱 전역 설정을 %AppData%/HarnessHub/settings.json에 저장하는 서비스.
/// </summary>
public sealed class AppSettingsService : IAppSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _settingsPath;
    private HarnessProvider _activeProvider;
    private int _contextWindowSize;

    public AppSettingsService()
    {
        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HarnessHub");

        Directory.CreateDirectory(appDataDir);
        _settingsPath = Path.Combine(appDataDir, "settings.json");

        Load();
    }

    /// <inheritdoc />
    public HarnessProvider ActiveProvider => _activeProvider;

    /// <inheritdoc />
    public int ContextWindowSize => _contextWindowSize;

    /// <inheritdoc />
    public void SetProvider(HarnessProvider provider)
    {
        if (_activeProvider == provider)
            return;

        _activeProvider = provider;
        Save();

        Log.Information("하네스 프로바이더 변경: {Provider}", provider);
        WeakReferenceMessenger.Default.Send(new HarnessProviderChangedMessage(provider));
    }

    /// <inheritdoc />
    public void SetContextWindowSize(int size)
    {
        if (_contextWindowSize == size)
            return;

        _contextWindowSize = size;
        Save();

        Log.Information("컨텍스트 윈도우 크기 변경: {Size}", size);
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                _activeProvider = HarnessProvider.ClaudeCode;
                _contextWindowSize = 200_000;
                return;
            }

            var json = File.ReadAllText(_settingsPath);
            var dto = JsonSerializer.Deserialize<AppSettingsDto>(json, JsonOptions);
            if (dto is null)
            {
                _activeProvider = HarnessProvider.ClaudeCode;
                _contextWindowSize = 200_000;
                return;
            }

            _activeProvider = Enum.TryParse<HarnessProvider>(dto.Provider, out var provider)
                ? provider
                : HarnessProvider.ClaudeCode;

            _contextWindowSize = dto.ContextWindowSize > 0
                ? dto.ContextWindowSize
                : 200_000;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "앱 설정 로드 실패, 기본값 사용");
            _activeProvider = HarnessProvider.ClaudeCode;
            _contextWindowSize = 200_000;
        }
    }

    private void Save()
    {
        try
        {
            var dto = new AppSettingsDto
            {
                Provider = _activeProvider.ToString(),
                ContextWindowSize = _contextWindowSize
            };

            var json = JsonSerializer.Serialize(dto, JsonOptions);
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "앱 설정 저장 실패");
        }
    }
}
