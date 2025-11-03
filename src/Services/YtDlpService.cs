using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using YTDLManager.Models;
using Newtonsoft.Json;
using Serilog;

namespace YTDLManager.Services;

public interface IYtDlpService
{
    string YtDlpPath { get; }
    string FfmpegPath { get; }
    Task<VideoInfo?> GetVideoInfoAsync(string url);
    Task<List<string>> GetPlaylistUrlsAsync(string playlistUrl);
    Process StartDownload(DownloadItem item, Action<string> outputCallback, Action<double> progressCallback);
}

public class YtDlpService : IYtDlpService
{
    private readonly string _appDataPath;
    
    public string YtDlpPath { get; }
    public string FfmpegPath { get; }

    public YtDlpService()
    {
        _appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YTDLManager", "bin");

        YtDlpPath = Path.Combine(_appDataPath, "yt-dlp.exe");
        FfmpegPath = Path.Combine(_appDataPath, "ffmpeg.exe");
    }

    public async Task<VideoInfo?> GetVideoInfoAsync(string url)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = YtDlpPath,
                    Arguments = $"--dump-json --no-playlist \"{url}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                Log.Error("Failed to get video info for URL: {Url}", url);
                return null;
            }

            var json = JsonConvert.DeserializeObject<dynamic>(output);
            
            return new VideoInfo
            {
                Id = json?.id ?? "",
                Title = json?.title ?? "Unknown",
                Description = json?.description ?? "",
                Uploader = json?.uploader ?? "",
                Thumbnail = json?.thumbnail ?? "",
                Duration = TimeSpan.FromSeconds((double)(json?.duration ?? 0)),
                ViewCount = json?.view_count ?? 0
            };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting video info for URL: {Url}", url);
            return null;
        }
    }

    public async Task<List<string>> GetPlaylistUrlsAsync(string playlistUrl)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = YtDlpPath,
                    Arguments = $"--flat-playlist --print url \"{playlistUrl}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                Log.Error("Failed to get playlist URLs");
                return new List<string>();
            }

            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting playlist URLs");
            return new List<string>();
        }
    }

    public Process StartDownload(DownloadItem item, Action<string> outputCallback, Action<double> progressCallback)
    {
        var args = BuildDownloadArguments(item);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = YtDlpPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            }
        };

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputCallback(e.Data);
                ParseProgress(e.Data, progressCallback);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputCallback(e.Data);
                ParseProgress(e.Data, progressCallback);
            }
        };

        return process;
    }

    private string BuildDownloadArguments(DownloadItem item)
    {
        var opts = item.Options;
        var args = new StringBuilder();

        // Output path
        var outputPath = Path.Combine(item.OutputPath, opts.OutputTemplate);
        args.Append($"-o \"{outputPath}\" ");

        // Quality settings
        if (opts.Quality == VideoQuality.AudioOnly)
        {
            args.Append("-x ");
            args.Append($"--audio-format {opts.AudioFormat.ToString().ToLower()} ");
        }
        else
        {
            var qualityString = opts.Quality switch
            {
                VideoQuality.Best => "bestvideo+bestaudio/best",
                VideoQuality.Quality4320p => "bestvideo[height<=4320]+bestaudio/best[height<=4320]",
                VideoQuality.Quality2160p => "bestvideo[height<=2160]+bestaudio/best[height<=2160]",
                VideoQuality.Quality1440p => "bestvideo[height<=1440]+bestaudio/best[height<=1440]",
                VideoQuality.Quality1080p => "bestvideo[height<=1080]+bestaudio/best[height<=1080]",
                VideoQuality.Quality720p => "bestvideo[height<=720]+bestaudio/best[height<=720]",
                VideoQuality.Quality480p => "bestvideo[height<=480]+bestaudio/best[height<=480]",
                VideoQuality.Quality360p => "bestvideo[height<=360]+bestaudio/best[height<=360]",
                VideoQuality.Quality240p => "bestvideo[height<=240]+bestaudio/best[height<=240]",
                VideoQuality.Quality144p => "bestvideo[height<=144]+bestaudio/best[height<=144]",
                _ => "bestvideo+bestaudio/best"
            };

            args.Append($"-f \"{qualityString}\" ");
        }

        // FFmpeg location
        args.Append($"--ffmpeg-location \"{Path.GetDirectoryName(FfmpegPath)}\" ");

        // Subtitles
        if (opts.DownloadSubtitles)
        {
            args.Append($"--write-subs --sub-langs {opts.SubtitleLanguages} ");
            if (opts.EmbedSubtitles)
                args.Append("--embed-subs ");
        }

        // Metadata
        if (opts.EmbedMetadata)
            args.Append("--embed-metadata ");

        if (opts.EmbedThumbnail)
            args.Append("--embed-thumbnail ");

        if (opts.EmbedChapters)
            args.Append("--embed-chapters ");

        // Playlist options
        if (opts.IsPlaylist)
        {
            if (opts.PlaylistStart.HasValue)
                args.Append($"--playlist-start {opts.PlaylistStart.Value} ");
            if (opts.PlaylistEnd.HasValue)
                args.Append($"--playlist-end {opts.PlaylistEnd.Value} ");
            if (opts.DownloadPlaylistReverse)
                args.Append("--playlist-reverse ");
        }
        else
        {
            args.Append("--no-playlist ");
        }

        // Performance options
        args.Append($"--concurrent-fragments {opts.MaxConcurrentFragments} ");

        // Speed limit
        if (opts.LimitSpeed && opts.SpeedLimitKBps > 0)
            args.Append($"--limit-rate {opts.SpeedLimitKBps}K ");

        // Proxy
        if (opts.UseProxy && !string.IsNullOrWhiteSpace(opts.ProxyUrl))
            args.Append($"--proxy \"{opts.ProxyUrl}\" ");

        // Progress and other flags
        args.Append("--progress --newline --no-warnings ");

        // Custom arguments
        if (!string.IsNullOrWhiteSpace(opts.CustomArguments))
            args.Append($"{opts.CustomArguments} ");

        // URL (always last)
        args.Append($"\"{item.Url}\"");

        Log.Debug("Download arguments: {Args}", args.ToString());
        return args.ToString();
    }

    private void ParseProgress(string output, Action<double> progressCallback)
    {
        // Parse progress from yt-dlp output
        // Format: [download]  45.2% of 123.45MiB at 1.23MiB/s ETA 00:12
        var progressMatch = Regex.Match(output, @"\[download\]\s+(\d+\.?\d*)%");
        if (progressMatch.Success)
        {
            if (double.TryParse(progressMatch.Groups[1].Value, out var progress))
            {
                progressCallback(progress);
            }
        }
    }
}
