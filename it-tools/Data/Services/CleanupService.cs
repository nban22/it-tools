namespace it_tools.Data.Services;

public class CleanupService
{
private readonly ILogger<CleanupService> _logger;

public CleanupService(ILogger<CleanupService> logger)
{
_logger = logger;
}

public async Task CleanupPendingDeletionsAsync()
{
string pendingFile = "pending-deletions.txt";
List<string> paths;

try
{
if (!File.Exists(pendingFile))
{
_logger.LogInformation("No pending-deletions.txt file found. Skipping cleanup.");
return;
}

paths = File.ReadAllLines(pendingFile)
.Where(p => !string.IsNullOrEmpty(p))
.Distinct()
.ToList();
_logger.LogInformation("Found {Count} paths in pending-deletions.txt", paths.Count);
}
catch (Exception ex)
{
_logger.LogError(ex, "Failed to read pending-deletions.txt. Skipping cleanup.");
return;
}

foreach (var path in paths)
{
bool deleted = await TryDeleteDirectory(path, maxRetries: 10, delayMs: 2000);
if (deleted)
{
_logger.LogInformation("Deleted pending folder: {Path}", path);
}
else
{
_logger.LogWarning("Failed to delete pending folder after retries: {Path}", path);
}
}

try
{
File.WriteAllText(pendingFile, string.Empty);
_logger.LogInformation("Cleared pending-deletions.txt.");
}
catch (Exception ex)
{
_logger.LogError(ex, "Failed to clear pending-deletions.txt.");
}
}

private async Task<bool> TryDeleteDirectory(string toolPath, int maxRetries = 10, int delayMs = 2000)
{
for (int i = 0; i < maxRetries; i++)
{
try
{
if (Directory.Exists(toolPath))
{
foreach (var file in Directory.GetFiles(toolPath, "*", SearchOption.AllDirectories))
{
try
{
File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None).Dispose();
_logger.LogInformation("File {File} is not locked in folder: {ToolPath}", file, toolPath);
}
catch (IOException ex)
{
_logger.LogWarning(ex, "File {File} is still locked in folder: {ToolPath}", file, toolPath);
throw;
}
}
Directory.Delete(toolPath, true);
_logger.LogInformation("Deleted pending folder: {ToolPath}", toolPath);
return true;
}
return true;
}
catch (Exception ex)
{
_logger.LogWarning(ex, "Attempt {Attempt} failed to delete pending folder: {ToolPath}. Retrying...", i + 1, toolPath);
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