namespace it_tools.Data.Services;

using it_tools.Data.DTOs;
using it_tools.Data.Repositories;
using Microsoft.AspNetCore.Components.Forms;
using System.IO.Compression;
using System.Text.Json;

public class ToolService : IToolService
{
    private readonly IToolRepository _toolRepository;
    private readonly ILogger<ToolService> _logger;

    public ToolService(
        IToolRepository toolRepository,
        ILogger<ToolService> logger)
    {
        _toolRepository = toolRepository;
        _logger = logger;
    }

    public async Task<ToolDto?> UploadToolAsync(IBrowserFile zipFile)
    {
        if (zipFile == null)
        {
            _logger.LogWarning("No ZIP file provided for upload.");
            return null;
        }

        string zipPath = Path.Combine("wwwroot/tools", zipFile.Name);
        string tempExtractPath = Path.Combine("wwwroot/tools", Path.GetRandomFileName());

        try
        {
            // Lưu file ZIP
            using (var stream = new FileStream(zipPath, FileMode.Create))
            {
                await zipFile.OpenReadStream().CopyToAsync(stream);
            }

            // Giải nén
            if (Directory.Exists(tempExtractPath))
            {
                Directory.Delete(tempExtractPath, true);
            }
            Directory.CreateDirectory(tempExtractPath);
            ZipFile.ExtractToDirectory(zipPath, tempExtractPath, true);

            // Kiểm tra nội dung ZIP
            string[] dllFiles = Directory.GetFiles(tempExtractPath, "*.dll", SearchOption.AllDirectories);
            string[] jsonFiles = Directory.GetFiles(tempExtractPath, "*.json", SearchOption.AllDirectories);

            if (dllFiles.Length != 1 || jsonFiles.Length != 1)
            {
                _logger.LogWarning("Invalid ZIP content: Found {DllCount} DLL files and {JsonCount} JSON files. Exactly one of each is required.", dllFiles.Length, jsonFiles.Length);
                return null;
            }

            string dllPath = dllFiles[0];
            string configPath = jsonFiles[0];
            ToolConfigDto? toolConfig = await ReadConfigFileAsync(configPath);

            if (toolConfig == null || string.IsNullOrEmpty(toolConfig.Slug))
            {
                _logger.LogWarning("Invalid or missing slug in config file: {ConfigPath}", configPath);
                return null;
            }

            // Xử lý và lưu tool
            string extractPath = Path.Combine("wwwroot/tools", toolConfig.Slug);
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
                _logger.LogInformation("Deleted old directory: {ExtractPath}", extractPath);
            }
            Directory.CreateDirectory(extractPath);

            string newDllPath = Path.Combine(extractPath, Path.GetFileName(dllPath));
            string newConfigPath = Path.Combine(extractPath, Path.GetFileName(configPath));
            File.Move(dllPath, newDllPath, true);
            File.Move(configPath, newConfigPath, true);

            var newTool = new ToolDto
            {
                Name = toolConfig.Name,
                Description = toolConfig.Description,
                Slug = toolConfig.Slug,
                Icon = toolConfig.Icon,
                DllPath = newDllPath,
                ConfigPath = newConfigPath
            };
            var toolGroup = new ToolGroupDto
            {
                Name = toolConfig.GroupName,
                Description = toolConfig.GroupDescription
            };

            return await _toolRepository.AddToolAsync(newTool, toolGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ZIP file: {FileName}", zipFile.Name);
            return null;
        }
        finally
        {
            // Dọn dẹp tài nguyên chỉ ở đây
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
                _logger.LogInformation("Deleted ZIP file: {ZipPath}", zipPath);
            }
            if (Directory.Exists(tempExtractPath))
            {
                Directory.Delete(tempExtractPath, true);
                _logger.LogInformation("Deleted temporary folder: {TempExtractPath}", tempExtractPath);
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private async Task<ToolConfigDto?> ReadConfigFileAsync(string configPath)
    {
        try
        {
            string jsonContent = await File.ReadAllTextAsync(configPath);
            return JsonSerializer.Deserialize<ToolConfigDto>(jsonContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading config file: {ConfigPath}", configPath);
            return null;
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

            if (string.IsNullOrEmpty(tool.Slug))
            {
                _logger.LogWarning("Tool slug is null or empty for tool with ID {ToolId}.", toolId);
                return false;
            }
            string toolPath = Path.Combine("wwwroot/tools", tool.Slug);
            if (Directory.Exists(toolPath))
            {
                Directory.Delete(toolPath, true);
            }

            return await _toolRepository.DeleteToolAsync(toolId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tool with ID {ToolId}", toolId);
            return false;
        }
    }
}