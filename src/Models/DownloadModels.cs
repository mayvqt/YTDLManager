using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YTDLManager.Models;

public enum DownloadStatus
{
    Pending,
    Downloading,
    Processing,
    Completed,
    Failed,
    Cancelled
}

public enum VideoQuality
{
    [Description("Best Available")]
    Best,
    [Description("8K (4320p)")]
    Quality4320p,
    [Description("4K (2160p)")]
    Quality2160p,
    [Description("1440p")]
    Quality1440p,
    [Description("1080p")]
    Quality1080p,
    [Description("720p")]
    Quality720p,
    [Description("480p")]
    Quality480p,
    [Description("360p")]
    Quality360p,
    [Description("240p")]
    Quality240p,
    [Description("144p")]
    Quality144p,
    [Description("Audio Only")]
    AudioOnly
}

public enum AudioFormat
{
    [Description("Best Available")]
    Best,
    MP3,
    AAC,
    FLAC,
    WAV,
    OPUS,
    M4A,
    VORBIS
}

public enum VideoCodec
{
    [Description("Best Available")]
    Best,
    [Description("H.264 (AVC)")]
    H264,
    [Description("H.265 (HEVC)")]
    H265,
    VP9,
    AV1
}

public class DownloadItem : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Url { get; set; } = string.Empty;
    
    private string _title = "Unknown";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
    
    public string Thumbnail { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    
    private DownloadStatus _status = DownloadStatus.Pending;
    public DownloadStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
    
    private double _progress = 0;
    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }
    
    public long TotalBytes { get; set; } = 0;
    public long DownloadedBytes { get; set; } = 0;
    public double DownloadSpeed { get; set; } = 0;
    public TimeSpan? EstimatedTimeRemaining { get; set; }
    public DateTime AddedTime { get; set; } = DateTime.Now;
    public DateTime? StartedTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    
    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }
    
    public DownloadOptions Options { get; set; } = new();
}

public class DownloadOptions
{
    public VideoQuality Quality { get; set; } = VideoQuality.Best;
    public AudioFormat AudioFormat { get; set; } = AudioFormat.Best;
    public VideoCodec VideoCodec { get; set; } = VideoCodec.Best;
    public bool DownloadSubtitles { get; set; } = false;
    public bool EmbedSubtitles { get; set; } = false;
    public bool EmbedThumbnail { get; set; } = true;
    public bool EmbedMetadata { get; set; } = true;
    public bool EmbedChapters { get; set; } = true;
    public string SubtitleLanguages { get; set; } = "en";
    public bool IsPlaylist { get; set; } = false;
    public int? PlaylistStart { get; set; }
    public int? PlaylistEnd { get; set; }
    public bool DownloadPlaylistReverse { get; set; } = false;
    public string CustomArguments { get; set; } = string.Empty;
    public int MaxConcurrentFragments { get; set; } = 5;
    public bool LimitSpeed { get; set; } = false;
    public long SpeedLimitKBps { get; set; } = 0;
    public bool UseProxy { get; set; } = false;
    public string ProxyUrl { get; set; } = string.Empty;
    public bool KeepVideo { get; set; } = false;
    public string OutputTemplate { get; set; } = "%(title)s.%(ext)s";
}

public class AppConfig
{
    public string DefaultDownloadPath { get; set; } = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "YTDLManager Downloads");
    public bool AutoCheckUpdates { get; set; } = true;
    public bool AutoUpdateYtDlp { get; set; } = true;
    public int MaxConcurrentDownloads { get; set; } = 3;
    public bool DarkTheme { get; set; } = true;
    public DownloadOptions DefaultOptions { get; set; } = new();
    public bool MinimizeToTray { get; set; } = false;
    public bool ShowNotifications { get; set; } = true;
    public string YtDlpPath { get; set; } = string.Empty;
    public string FfmpegPath { get; set; } = string.Empty;
}

public class VideoInfo
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Uploader { get; set; } = string.Empty;
    public string Thumbnail { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime? UploadDate { get; set; }
    public long ViewCount { get; set; }
    public List<FormatInfo> Formats { get; set; } = new();
    public List<SubtitleInfo> Subtitles { get; set; } = new();
    public bool IsPlaylist { get; set; }
    public int? PlaylistCount { get; set; }
}

public class FormatInfo
{
    public string FormatId { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? Fps { get; set; }
    public string VideoCodec { get; set; } = string.Empty;
    public string AudioCodec { get; set; } = string.Empty;
    public long? Filesize { get; set; }
    public int? Bitrate { get; set; }
}

public class SubtitleInfo
{
    public string Language { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public bool IsAutoGenerated { get; set; }
}
