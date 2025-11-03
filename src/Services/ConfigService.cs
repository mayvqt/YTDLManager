using System.IO;
using YTDLManager.Models;
using Newtonsoft.Json;
using Serilog;

namespace YTDLManager.Services;

public interface IConfigService
{
    AppConfig Config { get; }
    Task LoadConfigAsync();
    Task SaveConfigAsync();
}

public class ConfigService : IConfigService
{
    private readonly string _configPath;
    private AppConfig _config = new();

    public AppConfig Config => _config;

    public ConfigService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YTDLManager");
        
        Directory.CreateDirectory(appDataPath);
        _configPath = Path.Combine(appDataPath, "config.json");
    }

    public async Task LoadConfigAsync()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = await File.ReadAllTextAsync(_configPath);
                _config = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
                Log.Information("Configuration loaded successfully");
            }
            else
            {
                _config = new AppConfig();
                await SaveConfigAsync();
                Log.Information("Created new configuration file");
            }

            // Ensure download directory exists
            Directory.CreateDirectory(_config.DefaultDownloadPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load configuration");
            _config = new AppConfig();
        }
    }

    public async Task SaveConfigAsync()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
            await File.WriteAllTextAsync(_configPath, json);
            Log.Information("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save configuration");
        }
    }
}
