namespace it_tools.Data.Services;

using System.IO.Abstractions;

public class CleanupService : BackgroundService
{
    private readonly ILogger<CleanupService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IFileSystem _fileSystem;
    private readonly Dictionary<string, int> _folderRetryCounts = new();

    public CleanupService(ILogger<CleanupService> logger, IWebHostEnvironment environment, IFileSystem fileSystem)
    {
        _logger = logger;
        _environment = environment;
        _fileSystem = fileSystem;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CleanupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupPendingDeletionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup process.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("CleanupService stopped.");
    }

    public async Task CleanupPendingDeletionsAsync()
    {
        string pendingFile = Path.Combine(_environment.ContentRootPath, "pending-deletions.txt");
        if (!_fileSystem.File.Exists(pendingFile))
        {
            _logger.LogInformation("No pending-deletions.txt file found. Skipping cleanup.");
            return;
        }

        var foldersToDelete = new List<string>();
        try
        {
            var lines = await _fileSystem.File.ReadAllLinesAsync(pendingFile);
            foldersToDelete.AddRange(lines.Where(line => !string.IsNullOrWhiteSpace(line)).Distinct());
            _logger.LogInformation("Found {Count} paths in pending-deletions.txt", foldersToDelete.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read pending-deletions.txt. Skipping cleanup.");
            return;
        }

        if (!foldersToDelete.Any())
        {
            _logger.LogInformation("No folders to delete.");
            try
            {
                _fileSystem.File.Delete(pendingFile);
                _logger.LogInformation("Deleted empty pending-deletions.txt.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete empty pending-deletions.txt.");
            }
            return;
        }

        var remainingFolders = new List<string>(foldersToDelete);
        foreach (var path in foldersToDelete)
        {
            if (!_folderRetryCounts.ContainsKey(path))
            {
                _folderRetryCounts[path] = 0;
            }
            _folderRetryCounts[path]++;

            bool deleted = await TryDeleteDirectoryAsync(path, maxRetries: 20, delayMs: 4000); // Tăng số lần thử lên 20, thời gian chờ 4 giây
            if (deleted)
            {
                _logger.LogInformation("Deleted pending folder: {Path}", path);
                remainingFolders.Remove(path);
                _folderRetryCounts.Remove(path);
            }
            else
            {
                _logger.LogWarning("Failed to delete pending folder after retries: {Path}. Total attempts: {Attempts}. Will retry in next cycle.", path, _folderRetryCounts[path]);
                if (_folderRetryCounts[path] >= 50)
                {
                    _logger.LogError("Folder {Path} could not be deleted after {Attempts} attempts. Consider checking file locks or permissions manually, or restarting the application.", path, _folderRetryCounts[path]);
                    remainingFolders.Remove(path);
                    _folderRetryCounts.Remove(path);
                }
            }
        }

        try
        {
            if (remainingFolders.Any())
            {
                await _fileSystem.File.WriteAllLinesAsync(pendingFile, remainingFolders);
                _logger.LogInformation("Updated pending-deletions.txt with {Count} remaining folders.", remainingFolders.Count);
            }
            else
            {
                _fileSystem.File.Delete(pendingFile);
                _logger.LogInformation("All pending deletions completed, deleted pending-deletions.txt.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update pending-deletions.txt.");
        }
    }

    private async Task<bool> TryDeleteDirectoryAsync(string toolPath, int maxRetries = 20, int delayMs = 4000)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                if (_fileSystem.Directory.Exists(toolPath))
                {
                    _logger.LogInformation("Attempting to delete folder: {ToolPath}, attempt {Attempt}/{MaxRetries}", toolPath, i + 1, maxRetries);

                    try
                    {
                        _fileSystem.Directory.EnumerateFiles(toolPath).ToList();
                        _logger.LogInformation("Application has access to folder: {ToolPath}", toolPath);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        _logger.LogError(ex, "No permission to access folder: {ToolPath}. Please grant Full Control to the user running the application.", toolPath);
                        return false;
                    }

                    foreach (var file in _fileSystem.Directory.GetFiles(toolPath, "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            using (var stream = _fileSystem.File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                            {
                                _logger.LogInformation("File {File} is not locked in folder: {ToolPath}", file, toolPath);
                            }
                        }
                        catch (IOException ex)
                        {
                            _logger.LogWarning(ex, "File {File} is still locked in folder: {ToolPath}. Attempt {Attempt}/{MaxRetries}", file, toolPath, i + 1, maxRetries);
                            await Task.Delay(delayMs); // Chờ lâu hơn để file được giải phóng
                            continue;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            _logger.LogError(ex, "No permission to access file: {File} in folder: {ToolPath}. Please grant Full Control to the user running the application.", file, toolPath);
                            return false;
                        }
                    }

                    _fileSystem.Directory.Delete(toolPath, true);
                    _logger.LogInformation("Deleted pending folder: {ToolPath}", toolPath);
                    return true;
                }
                _logger.LogInformation("Folder {ToolPath} no longer exists.", toolPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Attempt {Attempt}/{MaxRetries} failed to delete pending folder: {ToolPath}. Retrying...", i + 1, maxRetries, toolPath);
                if (i == maxRetries - 1)
                {
                    return false;
                }
                await Task.Delay(delayMs);
            }
        }
        return false;
    }
}