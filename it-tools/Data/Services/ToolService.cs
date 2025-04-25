namespace it_tools.Data.Services;

using it_tools.Data.DTOs;
using it_tools.Data.Models;
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

    public async Task<ToolDto?> GetToolByIdAsync(string toolId)
    {
        if (string.IsNullOrEmpty(toolId))
        {
            throw new ArgumentException("Tool ID cannot be null or empty.", nameof(toolId));
        }

        if (!Guid.TryParse(toolId, out var toolGuid) || toolGuid == Guid.Empty)
        {
            throw new ArgumentException("Tool ID is not a valid GUID or is empty.", nameof(toolId));
        }

        var toolEntity = await _toolRepository.GetToolEntityByIdAsync(toolGuid);
        return toolEntity != null ? MapToolEntityToDto(toolEntity) : null;
    }

    public async Task<ToolDto?> GetToolBySlugAsync(string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            throw new ArgumentException("Tool slug cannot be null or empty.", nameof(slug));
        }

        var toolEntity = await _toolRepository.GetToolEntityBySlugAsync(slug);
        return toolEntity != null ? MapToolEntityToDto(toolEntity) : null;
    }

    public async Task<List<ToolDto>> GetAllToolsForUnauthorizedAsync()
    {
        var tools = await _toolRepository.GetAllEnabledToolEntitiesAsync();
        return tools.Select(t => MapToolEntityToDto(t, false)).ToList();
    }

    public async Task<List<ToolDto>> GetAllToolsForUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid user ID.", nameof(userId));
        }

        var tools = await _toolRepository.GetAllEnabledToolEntitiesAsync();
        var favoriteToolIds = await _toolRepository.GetFavoriteToolIdsForUserAsync(userGuid);

        return tools.Select(t => MapToolEntityToDto(t, favoriteToolIds.Contains(t.Id))).ToList();
    }

    public async Task<List<ToolGroupDto>> GetAllToolGroups()
    {
        var toolGroups = await _toolRepository.GetAllEnabledToolGroupEntitiesAsync();
        return toolGroups.Select(tg => MapToolGroupEntityToDto(tg, false)).ToList();
    }

    public async Task<ToolGroupDto> GetFavoriteToolGroup(string userId)
    {
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new ArgumentException("Invalid user ID.", nameof(userId));
        }

        var favoriteTools = await _toolRepository.GetFavoriteToolEntitiesForUserAsync(userGuid);
        
        return new ToolGroupDto
        {
            Id = "favorites",
            Name = "Favorite Tools",
            Description = "Your favorite tools collection",
            IsExpanded = true,
            Tools = favoriteTools.Select(t => MapToolEntityToDto(t, true)).ToList()
        };
    }

    public async Task<List<ToolGroupDto>> GetAllToolGroupsForAdminAsync()
    {
        var toolGroups = await _toolRepository.GetAllToolGroupEntitiesAsync();
        return toolGroups.Select(tg => MapToolGroupEntityToDto(tg, true)).ToList();
    }

    public async Task<bool> UpdateToolPremiumStatusAsync(string toolId, bool isPremium)
    {
        if (string.IsNullOrEmpty(toolId) || !Guid.TryParse(toolId, out var toolGuid))
        {
            throw new ArgumentException("Invalid tool ID.", nameof(toolId));
        }

        return await _toolRepository.UpdateToolPropertyAsync(toolGuid, nameof(Tool.IsPremium), isPremium);
    }

    public async Task<bool> UpdateToolEnabledStatusAsync(string toolId, bool isEnabled)
    {
        if (string.IsNullOrEmpty(toolId) || !Guid.TryParse(toolId, out var toolGuid))
        {
            throw new ArgumentException("Invalid tool ID.", nameof(toolId));
        }

        return await _toolRepository.UpdateToolPropertyAsync(toolGuid, nameof(Tool.IsEnabled), isEnabled);
    }

    public async Task<bool> DeleteToolAsync(string toolId)
    {
        try
        {
            if (string.IsNullOrEmpty(toolId) || !Guid.TryParse(toolId, out var toolGuid))
            {
                _logger.LogWarning("Invalid tool ID format: {ToolId}", toolId);
                return false;
            }

            var tool = await _toolRepository.GetToolEntityByIdAsync(toolGuid);
            if (tool == null)
            {
                _logger.LogWarning("Tool with ID {ToolId} not found.", toolId);
                return false;
            }

            // Unload the assembly before deleting
            if (!string.IsNullOrEmpty(tool.DllPath))
            {
                string toolPath = Path.GetDirectoryName(tool.DllPath) ?? string.Empty;
                await _toolAssemblyService.UnloadToolAssemblyAsync(tool.DllPath);
                
                // Delete from database
                var result = await _toolRepository.DeleteToolAsync(toolGuid);
                if (!result)
                {
                    _logger.LogWarning("Failed to delete tool with ID {ToolId} from database", toolId);
                    return false;
                }

                // Schedule the physical folder for deletion
                if (!string.IsNullOrEmpty(toolPath) && _fileSystem.Directory.Exists(toolPath))
                {
                    await ScheduleFolderDeletionAsync(toolPath);
                }
                
                return true;
            }
            else
            {
                // Just delete from database if no DLL path exists
                return await _toolRepository.DeleteToolAsync(toolGuid);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tool with ID {ToolId}", toolId);
            return false;
        }
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
            // Save the file
            using (var stream = file.OpenReadStream())
            using (var fileStream = _fileSystem.FileStream.New(dllPath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            var fileInfo = _fileSystem.FileInfo.New(dllPath);
            if (!_fileSystem.File.Exists(fileInfo.FullName))
            {
                throw new IOException("Could not verify the saved DLL file");
            }

            Assembly? assembly = null;
            try
            {
                // Load and extract metadata
                assembly = _toolAssemblyService.LoadToolAssembly(dllPath);
                var toolDto = _toolAssemblyService.ExtractToolMetadata(assembly);
                toolDto.DllPath = dllPath;
                toolDto.IsEnabled = false;
                
                if (toolDto.Group == null)
                {
                    throw new InvalidOperationException("Tool group cannot be null.");
                }

                if (string.IsNullOrEmpty(toolDto.Slug))
                {
                    throw new InvalidOperationException("Tool slug cannot be null or empty.");
                }

                // Check if tool already exists by slug
                var existingTool = await _toolRepository.GetToolEntityBySlugAsync(toolDto.Slug);
                if (existingTool != null)
                {
                    // Update existing tool
                    if (!string.IsNullOrEmpty(existingTool.DllPath))
                    {
                        string oldToolPath = Path.GetDirectoryName(existingTool.DllPath) ?? string.Empty;
                        if (_fileSystem.Directory.Exists(oldToolPath))
                        {
                            await _toolAssemblyService.UnloadToolAssemblyAsync(existingTool.DllPath);
                            await ScheduleFolderDeletionAsync(oldToolPath);
                        }
                    }

                    // Find or create the tool group
                    var toolGroup = await EnsureToolGroupExistsAsync(toolDto.Group);
                    
                    // Update the tool properties
                    existingTool.Name = toolDto.Name;
                    existingTool.Description = toolDto.Description;
                    existingTool.DllPath = dllPath;
                    existingTool.IsEnabled = toolDto.IsEnabled;
                    existingTool.IsPremium = toolDto.IsPremium;
                    existingTool.GroupId = toolGroup.Id;
                    existingTool.Icon = toolDto.Icon;
                    
                    await _toolRepository.UpdateToolAsync(existingTool);
                    return MapToolEntityToDto(existingTool);
                }
                else
                {
                    // Create new tool
                    var toolGroup = await EnsureToolGroupExistsAsync(toolDto.Group);
                    
                    var newTool = new Tool
                    {
                        GroupId = toolGroup.Id,
                        Name = toolDto.Name,
                        Description = toolDto.Description,
                        DllPath = toolDto.DllPath ?? string.Empty,
                        Slug = toolDto.Slug,
                        Icon = toolDto.Icon,
                        IsPremium = false,
                        IsEnabled = false
                    };
                    
                    var createdTool = await _toolRepository.CreateToolAsync(newTool);
                    return MapToolEntityToDto(createdTool);
                }
            }
            catch (BadImageFormatException ex)
            {
                _logger.LogError(ex, "Invalid DLL format. The file is not a valid .NET assembly");
                throw new InvalidOperationException("Invalid DLL file or not a .NET assembly", ex);
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
                    await _toolAssemblyService.UnloadToolAssemblyAsync(dllPath);
                    _fileSystem.Directory.Delete(toolFolder, true);
                }
            }
            catch (Exception cleanupEx)
            {
                _logger.LogWarning(cleanupEx, "Failed to clean up tool folder: {ToolFolder}. Scheduling for later deletion.", toolFolder);
                await ScheduleFolderDeletionAsync(toolFolder);
            }
            throw;
        }
    }

    public async Task UpdateToolAsync(ToolDto toolDto)
    {
        if (string.IsNullOrEmpty(toolDto.Id) || !Guid.TryParse(toolDto.Id, out var toolId))
        {
            throw new ArgumentException("Invalid tool ID.", nameof(toolDto.Id));
        }

        var existingTool = await _toolRepository.GetToolEntityByIdAsync(toolId);
        if (existingTool == null)
        {
            throw new InvalidOperationException($"Tool with ID {toolDto.Id} not found.");
        }

        // Find or create the tool group if specified
        if (toolDto.Group != null)
        {
            var toolGroup = await EnsureToolGroupExistsAsync(toolDto.Group);
            existingTool.GroupId = toolGroup.Id;
        }

        // Update tool properties
        existingTool.Name = toolDto.Name;
        existingTool.Description = toolDto.Description;
        existingTool.DllPath = toolDto.DllPath ?? throw new ArgumentNullException(nameof(toolDto.DllPath), "DllPath cannot be null");
        existingTool.Slug = toolDto.Slug;
        existingTool.Icon = toolDto.Icon;
        existingTool.IsPremium = toolDto.IsPremium;
        existingTool.IsEnabled = toolDto.IsEnabled;

        await _toolRepository.UpdateToolAsync(existingTool);
    }

    // Helper methods
    private async Task<ToolGroup> EnsureToolGroupExistsAsync(ToolGroupDto groupDto)
    {
        if (string.IsNullOrEmpty(groupDto.Name))
        {
            throw new ArgumentException("Tool group name cannot be empty", nameof(groupDto.Name));
        }

        var toolGroup = await _toolRepository.GetToolGroupByNameAsync(groupDto.Name);
        if (toolGroup == null)
        {
            toolGroup = new ToolGroup
            {
                Name = groupDto.Name,
                Description = groupDto.Description ?? string.Empty
            };
            return await _toolRepository.CreateToolGroupAsync(toolGroup);
        }
        return toolGroup;
    }

    private ToolDto MapToolEntityToDto(Tool tool, bool isFavorite = false)
    {
        return new ToolDto
        {
            Id = tool.Id.ToString(),
            GroupId = tool.GroupId.ToString(),
            Name = tool.Name,
            Description = tool.Description,
            DllPath = tool.DllPath,
            Slug = tool.Slug,
            Icon = tool.Icon,
            IsPremium = tool.IsPremium,
            IsEnabled = tool.IsEnabled,
            IsFavorite = isFavorite,
            Group = tool.Group != null ? new ToolGroupDto
            {
                Id = tool.Group.Id.ToString(),
                Name = tool.Group.Name,
                Description = tool.Group.Description
            } : null
        };
    }

    private ToolGroupDto MapToolGroupEntityToDto(ToolGroup toolGroup, bool includeDisabledTools)
    {
        var tools = includeDisabledTools
            ? toolGroup.Tools
            : toolGroup.Tools.Where(t => t.IsEnabled).ToList();

        return new ToolGroupDto
        {
            Id = toolGroup.Id.ToString(),
            Name = toolGroup.Name,
            Description = toolGroup.Description,
            IsExpanded = false,
            Tools = tools.Select(t => MapToolEntityToDto(t)).ToList()
        };
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