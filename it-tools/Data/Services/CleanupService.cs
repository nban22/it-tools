public class CleanupService
{
    private readonly ILogger<CleanupService> _logger;

    public CleanupService(ILogger<CleanupService> logger)
    {
        _logger = logger;
    }

    public void CleanupPendingDeletions()
    {
        string pendingFile = "pending-deletions.txt";
        if (!File.Exists(pendingFile)) return;

        var paths = File.ReadAllLines(pendingFile).Where(p => !string.IsNullOrEmpty(p)).ToList();
        foreach (var path in paths)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    _logger.LogInformation("Deleted pending folder: {Path}", path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete pending folder: {Path}", path);
            }
        }
        File.WriteAllText(pendingFile, string.Empty); // Xóa danh sách sau khi xử lý
    }
}