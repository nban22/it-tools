namespace it_tools.Data.Services;

using it_tools.Data.DTOs;
using it_tools.Data.Repositories;
using Microsoft.AspNetCore.Components.Forms;

public class ToolService : IToolService
{
    private readonly IToolRepository _toolRepository;
    private readonly ILogger<ToolService> _logger;
    private readonly ToolAssemblyService _toolAssemblyService;
    private readonly IWebHostEnvironment _environment;
    private readonly CleanupService _cleanupService;

    public ToolService(
        IToolRepository toolRepository,
        ILogger<ToolService> logger,
        IWebHostEnvironment environment,
        ToolAssemblyService toolAssemblyService,
        CleanupService cleanupService)
    {
        _toolRepository = toolRepository;
        _logger = logger;
        _environment = environment;
        _toolAssemblyService = toolAssemblyService;
        _cleanupService = cleanupService;
    }

    public async Task<ToolDto?> UploadToolAsync(IBrowserFile file)
    {
        if (!file.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Uploaded file must be a DLL file.", nameof(file));
        }
        string uploadsFolder = Path.Combine(_environment.ContentRootPath, "ToolPlugins");
        Directory.CreateDirectory(uploadsFolder);

        string uniqueFolderName = Guid.NewGuid().ToString();
        string toolFolder = Path.Combine(uploadsFolder, uniqueFolderName);

        Directory.CreateDirectory(toolFolder);
        try
        {
            string dllPath = Path.Combine(toolFolder, file.Name);

            using (var stream = file.OpenReadStream())
            using (var fileStream = new FileStream(dllPath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            var fileInfo = new FileInfo(dllPath);
            if (fileInfo.Exists)
            {
                _logger.LogInformation("Saved file verified. Size: {FileSize} bytes, Last write time: {LastWriteTime}",
                    fileInfo.Length, fileInfo.LastWriteTime);
            }
            else
            {
                throw new IOException("Không thể xác minh file DLL đã lưu");
            }

            try
            {
                var assembly = _toolAssemblyService.LoadToolAssembly(dllPath);
                var toolDto = _toolAssemblyService.ExtractToolMetadata(assembly);

                toolDto.DllPath = dllPath;
                toolDto.IsEnabled = false;

                if (toolDto.Group == null)
                {
                    throw new InvalidOperationException("Tool group cannot be null.");
                }
                var savedTool = await _toolRepository.AddToolAsync(toolDto, toolDto.Group);

                return savedTool;
            }
            catch (BadImageFormatException ex)
            {
                _logger.LogError(ex, "Invalid DLL format. The file is not a valid .NET assembly");
                throw new InvalidOperationException("File DLL không hợp lệ hoặc không phải là assembly .NET", ex);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing tool upload. Error type: {ErrorType}, Message: {ErrorMessage}",
                ex.GetType().Name, ex.Message);

            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerErrorType}, Message: {InnerErrorMessage}",
                    ex.InnerException.GetType().Name, ex.InnerException.Message);
            }

            _logger.LogInformation("Cleaning up tool folder: {ToolFolder}", toolFolder);
            try
            {
                Directory.Delete(toolFolder, true);
                _logger.LogInformation("Tool folder cleanup successful");
            }
            catch (Exception cleanupEx)
            {
                _logger.LogWarning(cleanupEx, "Failed to clean up tool folder");
            }
            throw;
        }
    }

    public async Task<bool> DeleteToolAsync(string toolId)
    {
        try
        {
            var tool = await _toolRepository.GetToolByIdAsync(toolId);
            if (tool == null)
            {
                _logger.LogWarning("Tool with ID {ToolId} not found.", toolId);
                return false;
            }

            if (!string.IsNullOrEmpty(tool.DllPath))
            {
                _toolAssemblyService.UnloadToolAssembly(tool.DllPath);
            }

            string toolPath = Path.GetDirectoryName(tool.DllPath) ?? string.Empty;
            if (Directory.Exists(toolPath))
            {
                bool deleted = await TryDeleteDirectoryAsync(toolPath, maxRetries: 2, delayMs: 0);
                if (!deleted)
                {
                    _logger.LogWarning("Failed to delete tool folder immediately. Running CleanupService...");
                    await _cleanupService.CleanupPendingDeletionsAsync();
                }
            }

            var result = await _toolRepository.DeleteToolAsync(toolId);
            if (result)
            {
                _logger.LogInformation("Successfully deleted tool with ID {ToolId}", toolId);
            }
            else
            {
                _logger.LogWarning("Failed to delete tool with ID {ToolId} from database", toolId);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tool with ID {ToolId}", toolId);
            return false;
        }
    }

    private async Task<bool> TryDeleteDirectoryAsync(string toolPath, int maxRetries = 2, int delayMs = 0)
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
                            throw new UnauthorizedAccessException($"File {file} is locked.", ex);
                        }
                    }

                    Directory.Delete(toolPath, true);
                    _logger.LogInformation("Deleted tool folder: {ToolPath}", toolPath);
                    return true;
                }
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Attempt {Attempt} failed to delete tool folder: {ToolPath}. Retrying...", i + 1, toolPath);
                if (i == maxRetries - 1)
                {
                    _logger.LogWarning("Failed to delete tool folder after {MaxRetries} attempts. Scheduling for deletion: {ToolPath}", maxRetries, toolPath);
                    try
                    {
                        await File.AppendAllTextAsync("pending-deletions.txt", toolPath + Environment.NewLine);
                    }
                    catch (Exception writeEx)
                    {
                        _logger.LogError(writeEx, "Failed to schedule deletion for {ToolPath}.", toolPath);
                    }
                    return false;
                }
                await Task.Delay(delayMs);
            }
        }
        return false;
    }
}