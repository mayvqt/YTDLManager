using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YTDLManager.Models;
using YTDLManager.Services;
using Serilog;

namespace YTDLManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IConfigService _configService;
    private readonly IUpdateService _updateService;
    private readonly IYtDlpService _ytDlpService;
    private readonly IDownloadService _downloadService;

    [ObservableProperty]
    private string _videoUrl = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isUpdating;

    [ObservableProperty]
    private double _updateProgress;

    [ObservableProperty]
    private string _ytDlpVersion = "Checking...";

    [ObservableProperty]
    private VideoQuality _selectedQuality = VideoQuality.Best;

    [ObservableProperty]
    private AudioFormat _selectedAudioFormat = AudioFormat.Best;

    [ObservableProperty]
    private VideoCodec _selectedCodec = VideoCodec.Best;

    [ObservableProperty]
    private bool _downloadSubtitles;

    [ObservableProperty]
    private bool _embedSubtitles;

    [ObservableProperty]
    private bool _embedThumbnail = true;

    [ObservableProperty]
    private bool _embedMetadata = true;

    [ObservableProperty]
    private string _outputPath = string.Empty;

    [ObservableProperty]
    private bool _isPlaylist;

    [ObservableProperty]
    private bool _audioOnly;

    [ObservableProperty]
    private string _customArguments = string.Empty;

    public ObservableCollection<DownloadItem> Downloads { get; } = new();
    public ObservableCollection<VideoQuality> QualityOptions { get; } = new(Enum.GetValues<VideoQuality>());
    public ObservableCollection<AudioFormat> AudioFormatOptions { get; } = new(Enum.GetValues<AudioFormat>());
    public ObservableCollection<VideoCodec> CodecOptions { get; } = new(Enum.GetValues<VideoCodec>());

    public MainViewModel(
        IConfigService configService,
        IUpdateService updateService,
        IYtDlpService ytDlpService,
        IDownloadService downloadService)
    {
        _configService = configService;
        _updateService = updateService;
        _ytDlpService = ytDlpService;
        _downloadService = downloadService;

        // Subscribe to download events
        _downloadService.DownloadAdded += OnDownloadAdded;
        _downloadService.DownloadUpdated += OnDownloadUpdated;
        _downloadService.DownloadCompleted += OnDownloadCompleted;
        _downloadService.DownloadFailed += OnDownloadFailed;

        // Initialize asynchronously with proper error handling
        _ = Task.Run(async () =>
        {
            try
            {
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize MainViewModel");
                StatusMessage = "Initialization failed";
            }
        });
    }

    private async Task InitializeAsync()
    {
        await _configService.LoadConfigAsync();
        
        OutputPath = _configService.Config.DefaultDownloadPath;
        SelectedQuality = _configService.Config.DefaultOptions.Quality;
        SelectedAudioFormat = _configService.Config.DefaultOptions.AudioFormat;
        SelectedCodec = _configService.Config.DefaultOptions.VideoCodec;
        DownloadSubtitles = _configService.Config.DefaultOptions.DownloadSubtitles;
        EmbedSubtitles = _configService.Config.DefaultOptions.EmbedSubtitles;
        EmbedThumbnail = _configService.Config.DefaultOptions.EmbedThumbnail;
        EmbedMetadata = _configService.Config.DefaultOptions.EmbedMetadata;

        _downloadService.SetMaxConcurrentDownloads(_configService.Config.MaxConcurrentDownloads);

        await CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            StatusMessage = "Checking for updates...";

            var needsUpdate = await _updateService.CheckForYtDlpUpdateAsync();
            if (needsUpdate)
            {
                await UpdateYtDlpAsync();
            }

            var hasFfmpeg = await _updateService.CheckForFfmpegAsync();
            if (!hasFfmpeg)
            {
                StatusMessage = "Downloading ffmpeg...";
                await _updateService.DownloadFfmpegAsync(new Progress<double>(p => UpdateProgress = p));
            }

            YtDlpVersion = await _updateService.GetYtDlpVersionAsync();
            StatusMessage = "Ready";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking for updates");
            StatusMessage = "Error checking for updates";
        }
    }

    [RelayCommand]
    private async Task UpdateYtDlpAsync()
    {
        try
        {
            IsUpdating = true;
            StatusMessage = "Updating yt-dlp...";

            var progress = new Progress<double>(p => UpdateProgress = p);
            var success = await _updateService.UpdateYtDlpAsync(progress);

            if (success)
            {
                YtDlpVersion = await _updateService.GetYtDlpVersionAsync();
                StatusMessage = "Update completed successfully";
            }
            else
            {
                StatusMessage = "Update failed";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating yt-dlp");
            StatusMessage = "Error updating yt-dlp";
        }
        finally
        {
            IsUpdating = false;
        }
    }

    [RelayCommand]
    private async Task AddDownloadAsync()
    {
        if (string.IsNullOrWhiteSpace(VideoUrl))
        {
            StatusMessage = "Please enter a URL";
            return;
        }

        try
        {
            StatusMessage = "Fetching video information...";

            var videoInfo = await _ytDlpService.GetVideoInfoAsync(VideoUrl);
            if (videoInfo == null)
            {
                StatusMessage = "Failed to fetch video information";
                return;
            }

            var downloadItem = new DownloadItem
            {
                Url = VideoUrl,
                Title = videoInfo.Title,
                Thumbnail = videoInfo.Thumbnail,
                OutputPath = OutputPath,
                Options = new DownloadOptions
                {
                    Quality = AudioOnly ? VideoQuality.AudioOnly : SelectedQuality,
                    AudioFormat = SelectedAudioFormat,
                    VideoCodec = SelectedCodec,
                    DownloadSubtitles = DownloadSubtitles,
                    EmbedSubtitles = EmbedSubtitles,
                    EmbedThumbnail = EmbedThumbnail,
                    EmbedMetadata = EmbedMetadata,
                    IsPlaylist = IsPlaylist,
                    CustomArguments = CustomArguments
                }
            };

            await _downloadService.AddDownloadAsync(downloadItem);
            
            VideoUrl = string.Empty;
            StatusMessage = $"Added: {videoInfo.Title}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding download");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CancelDownloadAsync(string id)
    {
        await _downloadService.CancelDownloadAsync(id);
    }

    [RelayCommand]
    private async Task RemoveDownloadAsync(string id)
    {
        await _downloadService.RemoveDownloadAsync(id);
        var item = Downloads.FirstOrDefault(d => d.Id == id);
        if (item != null)
        {
            Application.Current.Dispatcher.Invoke(() => Downloads.Remove(item));
        }
    }

    [RelayCommand]
    private async Task ClearCompletedAsync()
    {
        await _downloadService.ClearCompletedAsync();
        var completed = Downloads.Where(d => 
            d.Status == DownloadStatus.Completed || 
            d.Status == DownloadStatus.Failed || 
            d.Status == DownloadStatus.Cancelled).ToList();
        
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (var item in completed)
            {
                Downloads.Remove(item);
            }
        });
    }

    [RelayCommand]
    private void BrowseOutputPath()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Select Download Folder",
            Filter = "Folder|*.*",
            FileName = "Select Folder"
        };

        // Workaround to select folder
        if (dialog.ShowDialog() == true)
        {
            OutputPath = System.IO.Path.GetDirectoryName(dialog.FileName) ?? OutputPath;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        _configService.Config.DefaultDownloadPath = OutputPath;
        _configService.Config.DefaultOptions.Quality = SelectedQuality;
        _configService.Config.DefaultOptions.AudioFormat = SelectedAudioFormat;
        _configService.Config.DefaultOptions.VideoCodec = SelectedCodec;
        _configService.Config.DefaultOptions.DownloadSubtitles = DownloadSubtitles;
        _configService.Config.DefaultOptions.EmbedSubtitles = EmbedSubtitles;
        _configService.Config.DefaultOptions.EmbedThumbnail = EmbedThumbnail;
        _configService.Config.DefaultOptions.EmbedMetadata = EmbedMetadata;

        await _configService.SaveConfigAsync();
        StatusMessage = "Settings saved";
    }

    private void OnDownloadAdded(object? sender, DownloadItem item)
    {
        Application.Current.Dispatcher.Invoke(() => Downloads.Add(item));
    }

    private void OnDownloadUpdated(object? sender, DownloadItem item)
    {
        // UI is already bound to the item, so just trigger property changes
    }

    private void OnDownloadCompleted(object? sender, DownloadItem item)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            StatusMessage = $"Completed: {item.Title}";
        });
    }

    private void OnDownloadFailed(object? sender, DownloadItem item)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            StatusMessage = $"Failed: {item.Title} - {item.ErrorMessage}";
        });
    }
}
