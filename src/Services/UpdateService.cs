using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Serilog;

namespace YTDLManager.Services;

public interface IUpdateService
{
    Task<bool> CheckForYtDlpUpdateAsync();
    Task<bool> UpdateYtDlpAsync(IProgress<double>? progress = null);
    Task<bool> CheckForFfmpegAsync();
    Task<bool> DownloadFfmpegAsync(IProgress<double>? progress = null);
    Task<string> GetYtDlpVersionAsync();
}

public class UpdateService : IUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly string _appDataPath;
    private readonly string _ytDlpPath;
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;

    public UpdateService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("YTDLManager/1.0");

        _appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YTDLManager", "bin");
        
        Directory.CreateDirectory(_appDataPath);

        _ytDlpPath = Path.Combine(_appDataPath, "yt-dlp.exe");
        _ffmpegPath = Path.Combine(_appDataPath, "ffmpeg.exe");
        _ffprobePath = Path.Combine(_appDataPath, "ffprobe.exe");
    }

    public async Task<bool> CheckForYtDlpUpdateAsync()
    {
        try
        {
            if (!File.Exists(_ytDlpPath))
            {
                Log.Information("yt-dlp not found, update needed");
                return true;
            }

            var currentVersion = await GetYtDlpVersionAsync();
            Log.Information("Current yt-dlp version: {Version}", currentVersion);
            
            // Check GitHub for latest version
            var response = await _httpClient.GetStringAsync(
                "https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest");
            
            var latestVersion = System.Text.Json.JsonDocument.Parse(response)
                .RootElement.GetProperty("tag_name").GetString();

            Log.Information("Latest yt-dlp version: {Version}", latestVersion);
            
            return currentVersion != latestVersion;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to check for yt-dlp update");
            return false;
        }
    }

    public async Task<bool> UpdateYtDlpAsync(IProgress<double>? progress = null)
    {
        try
        {
            Log.Information("Downloading yt-dlp...");
            const string downloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";

            var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var downloadedBytes = 0L;

            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(_ytDlpPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                downloadedBytes += bytesRead;

                if (totalBytes > 0)
                {
                    progress?.Report((double)downloadedBytes / totalBytes * 100);
                }
            }

            Log.Information("yt-dlp downloaded successfully");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to download yt-dlp");
            return false;
        }
    }

    public async Task<bool> CheckForFfmpegAsync()
    {
        return await Task.Run(() => File.Exists(_ffmpegPath) && File.Exists(_ffprobePath));
    }

    public async Task<bool> DownloadFfmpegAsync(IProgress<double>? progress = null)
    {
        try
        {
            Log.Information("Downloading ffmpeg...");
            
            // Download ffmpeg from gyan.dev (official Windows builds)
            const string downloadUrl = "https://github.com/GyanD/codexffmpeg/releases/download/7.0.2/ffmpeg-7.0.2-essentials_build.zip";

            var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var tempZipPath = Path.Combine(Path.GetTempPath(), "ffmpeg.zip");
            
            await using (var contentStream = await response.Content.ReadAsStreamAsync())
            await using (var fileStream = new FileStream(tempZipPath, FileMode.Create))
            {
                await contentStream.CopyToAsync(fileStream);
            }

            // Extract ffmpeg.exe and ffprobe.exe
            System.IO.Compression.ZipFile.ExtractToDirectory(tempZipPath, Path.GetTempPath(), true);
            
            var extractedFolder = Path.Combine(Path.GetTempPath(), "ffmpeg-7.0.2-essentials_build", "bin");
            
            if (File.Exists(Path.Combine(extractedFolder, "ffmpeg.exe")))
            {
                File.Copy(Path.Combine(extractedFolder, "ffmpeg.exe"), _ffmpegPath, true);
                File.Copy(Path.Combine(extractedFolder, "ffprobe.exe"), _ffprobePath, true);
            }

            // Cleanup
            File.Delete(tempZipPath);
            Directory.Delete(Path.Combine(Path.GetTempPath(), "ffmpeg-7.0.2-essentials_build"), true);

            Log.Information("ffmpeg downloaded successfully");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to download ffmpeg");
            return false;
        }
    }

    public async Task<string> GetYtDlpVersionAsync()
    {
        try
        {
            if (!File.Exists(_ytDlpPath))
                return "Not installed";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ytDlpPath,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var version = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return version.Trim();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get yt-dlp version");
            return "Unknown";
        }
    }
}
