namespace it_tools.Data.Services;

using it_tools.Data.DTOs;
using it_tools.Data.Repositories;
using Microsoft.AspNetCore.Components.Forms;
using System.Reflection;
using System.IO.Abstractions;

public class ToolService : IToolService
{
    private readonly IToolRepository _toolRepository;
    private readonly ILogger<ToolService> _logger;
    private readonly ToolAssemblyService _toolAssemblyService;
    private readonly IWebHostEnvironment _environment;
    private readonly CleanupService _cleanupService;
    private readonly IFileSystem _fileSystem;

    public ToolService(
        IToolRepository toolRepository,
        ILogger<ToolService> logger,
        IWebHostEnvironment environment,
        ToolAssemblyService toolAssemblyService,
        CleanupService cleanupService,
        IFileSystem fileSystem)
    {
        _toolRepository = toolRepository;
        _logger = logger;
        _environment = environment;
        _toolAssemblyService = toolAssemblyService;
        _cleanupService = cleanupService;
        _fileSystem = fileSystem;
    }

    public async Task<ToolDto?> UploadToolAsync(IBrowserFile file)
    {
        if (!file.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Uploaded file must be a DLL file.", nameof(file));
        }

        string uploadsFolder = Path.Combine(_environment.ContentRootPath, "ToolPlugins");
        _fileSystem.Directory.CreateDirectory(uploadsFolder);

        string uniqueFolderName = Guid.NewGuid().ToString();
        string toolFolder = Path.Combine(uploadsFolder, uniqueFolderName);
        _fileSystem.Directory.CreateDirectory(toolFolder);

        string dllPath = Path.Combine(toolFolder, file.Name);

        try
        {
            using (var stream = file.OpenReadStream())
            using (var fileStream = _fileSystem.FileStream.New(dllPath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            var fileInfo = _fileSystem.FileInfo.New(dllPath);
            if (!_fileSystem.File.Exists(fileInfo.FullName))
            {
                throw new IOException("Không thể xác minh file DLL đã lưu");
            }

            Assembly? assembly = null;
            try
            {
                assembly = _toolAssemblyService.LoadToolAssembly(dllPath);
                var toolDto = _toolAssemblyService.ExtractToolMetadata(assembly);

                toolDto.DllPath = dllPath;
                toolDto.IsEnabled = false;

                if (toolDto.Group == null)
                {
                    throw new InvalidOperationException("Tool group cannot be null.");
                }

                var existingTool = await _toolRepository.GetToolBySlugAsync(toolDto.Slug ?? throw new InvalidOperationException("Tool slug cannot be null."));
                if (existingTool != null)
                {
                    if (!string.IsNullOrEmpty(existingTool.DllPath))
                    {
                        string oldToolPath = Path.GetDirectoryName(existingTool.DllPath) ?? string.Empty;
                        if (_fileSystem.Directory.Exists(oldToolPath))
                        {
                            await _toolAssemblyService.UnloadToolAssemblyAsync(existingTool.DllPath);
                            await ScheduleFolderDeletionAsync(oldToolPath);
                        }
                    }

                    existingTool.Name = toolDto.Name;
                    existingTool.Description = toolDto.Description;
                    existingTool.DllPath = dllPath;
                    existingTool.IsEnabled = toolDto.IsEnabled;
                    existingTool.IsPremium = toolDto.IsPremium;
                    existingTool.Group = toolDto.Group;
                    await _toolRepository.UpdateToolAsync(existingTool);
                    return existingTool;
                }
                else
                {
                    var savedTool = await _toolRepository.AddToolAsync(toolDto, toolDto.Group);
                    return savedTool;
                }
            }
            catch (BadImageFormatException ex)
            {
                _logger.LogError(ex, "Invalid DLL format. The file is not a valid .NET assembly");
                throw new InvalidOperationException("File DLL không hợp lệ hoặc không phải là assembly .NET", ex);
            }
            finally
            {
                if (assembly != null)
                {
                    await _toolAssemblyService.UnloadToolAssemblyAsync(dllPath);
                }
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

            try
            {
                if (_fileSystem.Directory.Exists(toolFolder))
                {
                    await _toolAssemblyService.UnloadToolAssemblyAsync(dllPath); // Đảm bảo unload trước khi xóa
                    _fileSystem.Directory.Delete(toolFolder, true);
                }
            }
            catch (Exception cleanupEx)
            {
                _logger.LogWarning(cleanupEx, "Failed to clean up tool folder: {ToolFolder}. Scheduling for later deletion.", toolFolder);
                await ScheduleFolderDeletionAsync(toolFolder); // Lập lịch xóa nếu không xóa được ngay
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

            string toolPath = string.Empty;
            if (!string.IsNullOrEmpty(tool.DllPath))
            {
                toolPath = Path.GetDirectoryName(tool.DllPath) ?? string.Empty;
                await _toolAssemblyService.UnloadToolAssemblyAsync(tool.DllPath);
            }

            var result = await _toolRepository.DeleteToolAsync(toolId);
            if (!result)
            {
                _logger.LogWarning("Failed to delete tool with ID {ToolId} from database", toolId);
                return false;
            }


            if (!string.IsNullOrEmpty(toolPath) && _fileSystem.Directory.Exists(toolPath))
            {
                await ScheduleFolderDeletionAsync(toolPath);
            }
            else
            {
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tool with ID {ToolId}", toolId);
            return false;
        }
    }

    private async Task ScheduleFolderDeletionAsync(string toolPath)
    {
        try
        {
            string pendingFile = Path.Combine(_environment.ContentRootPath, "pending-deletions.txt");
            await _fileSystem.File.AppendAllTextAsync(pendingFile, toolPath + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule folder deletion for {ToolPath}", toolPath);
        }
    }
}