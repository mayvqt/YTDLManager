using System.Collections.Concurrent;
using System.Diagnostics;
using YTDLManager.Models;
using Serilog;

namespace YTDLManager.Services;

public interface IDownloadService
{
    event EventHandler<DownloadItem>? DownloadAdded;
    event EventHandler<DownloadItem>? DownloadUpdated;
    event EventHandler<DownloadItem>? DownloadCompleted;
    event EventHandler<DownloadItem>? DownloadFailed;

    IReadOnlyList<DownloadItem> Downloads { get; }
    Task AddDownloadAsync(DownloadItem item);
    Task CancelDownloadAsync(string id);
    Task RemoveDownloadAsync(string id);
    Task ClearCompletedAsync();
    void SetMaxConcurrentDownloads(int max);
}

public class DownloadService : IDownloadService
{
    private readonly IYtDlpService _ytDlpService;
    private readonly ConcurrentDictionary<string, DownloadItem> _downloads = new();
    private readonly ConcurrentDictionary<string, Process> _processes = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new();
    private readonly SemaphoreSlim _downloadSemaphore;
    private int _maxConcurrentDownloads = 3;

    public event EventHandler<DownloadItem>? DownloadAdded;
    public event EventHandler<DownloadItem>? DownloadUpdated;
    public event EventHandler<DownloadItem>? DownloadCompleted;
    public event EventHandler<DownloadItem>? DownloadFailed;

    public IReadOnlyList<DownloadItem> Downloads => _downloads.Values.ToList();

    public DownloadService(IYtDlpService ytDlpService)
    {
        _ytDlpService = ytDlpService;
        _downloadSemaphore = new SemaphoreSlim(_maxConcurrentDownloads, _maxConcurrentDownloads);
    }

    public void SetMaxConcurrentDownloads(int max)
    {
        _maxConcurrentDownloads = Math.Max(1, max);
    }

    public async Task AddDownloadAsync(DownloadItem item)
    {
        if (_downloads.TryAdd(item.Id, item))
        {
            DownloadAdded?.Invoke(this, item);
            Log.Information("Added download: {Title}", item.Title);

            // Start download in background
            _ = Task.Run(() => ProcessDownloadAsync(item));
        }
    }

    private async Task ProcessDownloadAsync(DownloadItem item)
    {
        var cts = new CancellationTokenSource();
        _cancellationTokens[item.Id] = cts;

        try
        {
            // Wait for available slot
            await _downloadSemaphore.WaitAsync(cts.Token);

            if (cts.Token.IsCancellationRequested)
            {
                UpdateDownloadStatus(item, DownloadStatus.Cancelled);
                return;
            }

            UpdateDownloadStatus(item, DownloadStatus.Downloading);
            item.StartedTime = DateTime.Now;

            var process = _ytDlpService.StartDownload(
                item,
                output => Log.Debug("Download output: {Output}", output),
                progress =>
                {
                    item.Progress = progress;
                    DownloadUpdated?.Invoke(this, item);
                });

            _processes[item.Id] = process;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cts.Token);

            if (process.ExitCode == 0)
            {
                item.CompletedTime = DateTime.Now;
                UpdateDownloadStatus(item, DownloadStatus.Completed);
                DownloadCompleted?.Invoke(this, item);
                Log.Information("Download completed: {Title}", item.Title);
            }
            else
            {
                item.ErrorMessage = $"Process exited with code {process.ExitCode}";
                UpdateDownloadStatus(item, DownloadStatus.Failed);
                DownloadFailed?.Invoke(this, item);
                Log.Error("Download failed: {Title} - {Error}", item.Title, item.ErrorMessage);
            }
        }
        catch (OperationCanceledException)
        {
            UpdateDownloadStatus(item, DownloadStatus.Cancelled);
            Log.Information("Download cancelled: {Title}", item.Title);
        }
        catch (Exception ex)
        {
            item.ErrorMessage = ex.Message;
            UpdateDownloadStatus(item, DownloadStatus.Failed);
            DownloadFailed?.Invoke(this, item);
            Log.Error(ex, "Download error: {Title}", item.Title);
        }
        finally
        {
            _downloadSemaphore.Release();
            _processes.TryRemove(item.Id, out _);
            _cancellationTokens.TryRemove(item.Id, out _);
        }
    }

    public async Task CancelDownloadAsync(string id)
    {
        if (_cancellationTokens.TryGetValue(id, out var cts))
        {
            cts.Cancel();

            if (_processes.TryGetValue(id, out var process))
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(true);
                        await process.WaitForExitAsync();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error killing process for download: {Id}", id);
                }
            }

            if (_downloads.TryGetValue(id, out var item))
            {
                UpdateDownloadStatus(item, DownloadStatus.Cancelled);
            }
        }
    }

    public Task RemoveDownloadAsync(string id)
    {
        if (_downloads.TryRemove(id, out var item))
        {
            Log.Information("Removed download: {Title}", item.Title);
        }
        return Task.CompletedTask;
    }

    public Task ClearCompletedAsync()
    {
        var completedIds = _downloads.Values
            .Where(d => d.Status == DownloadStatus.Completed || 
                       d.Status == DownloadStatus.Failed || 
                       d.Status == DownloadStatus.Cancelled)
            .Select(d => d.Id)
            .ToList();

        foreach (var id in completedIds)
        {
            _downloads.TryRemove(id, out _);
        }

        Log.Information("Cleared {Count} completed downloads", completedIds.Count);
        return Task.CompletedTask;
    }

    private void UpdateDownloadStatus(DownloadItem item, DownloadStatus status)
    {
        item.Status = status;
        DownloadUpdated?.Invoke(this, item);
    }
}
