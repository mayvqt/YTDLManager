using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YTDLManager.Services;
using YTDLManager.ViewModels;
using YTDLManager.Views;
using Serilog;

namespace YTDLManager;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        try
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "YTDLManager", "Logs", "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Register Services
                    services.AddSingleton<IConfigService, ConfigService>();
                    services.AddSingleton<IUpdateService, UpdateService>();
                    services.AddSingleton<IYtDlpService, YtDlpService>();
                    services.AddSingleton<IDownloadService, DownloadService>();
                    
                    // Register ViewModels
                    services.AddTransient<MainViewModel>();
                    
                    // Register Views
                    services.AddTransient<MainWindow>();
                })
                .UseSerilog()
                .Build();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Failed to initialize application");
            throw;
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            Log.Information("Starting host...");
            await _host.StartAsync();

            Log.Information("Getting main window from DI container...");
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            
            Log.Information("Showing main window...");
            mainWindow.Show();

            Log.Information("Calling base OnStartup...");
            base.OnStartup(e);
            
            Log.Information("Application startup completed successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
            MessageBox.Show($"Application failed to start: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
