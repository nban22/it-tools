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

    public ToolService(
        IToolRepository toolRepository,
        ILogger<ToolService> logger,
        IWebHostEnvironment environment,
        ToolAssemblyService toolAssemblyService)
    {
        _toolRepository = toolRepository;
        _logger = logger;
        _environment = environment;
        _toolAssemblyService = toolAssemblyService;
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

            // Đọc và lưu file DLL
            using (var stream = file.OpenReadStream())
            using (var fileStream = new FileStream(dllPath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }
            // Verify the saved file
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

            // Tải assembly và đọc metadata
            try
            {
                var assembly = _toolAssemblyService.LoadToolAssembly(dllPath);

                var toolDto = _toolAssemblyService.ExtractToolMetadata(assembly);

                toolDto.DllPath = dllPath;
                toolDto.IsEnabled = false;

                // Lưu vào database
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

            // Unload assembly trước khi xóa
            if (!string.IsNullOrEmpty(tool.DllPath))
            {
                _toolAssemblyService.UnloadToolAssembly(tool.DllPath);
            }

            // Xóa thư mục chứa file DLL
            string toolPath = Path.GetDirectoryName(tool.DllPath) ?? string.Empty;
            if (Directory.Exists(toolPath))
            {
                Directory.Delete(toolPath, true);
                _logger.LogInformation("Deleted tool folder: {ToolPath}", toolPath);
            }

            // Xóa bản ghi trong cơ sở dữ liệu
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
}