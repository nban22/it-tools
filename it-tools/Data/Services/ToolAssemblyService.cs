using System.Reflection;
using System.Runtime.Loader;
using it_tools.Data.DTOs;
using it_tools.ToolDevelopment.Attributes;
using it_tools.ToolDevelopment.Interfaces;
using System.IO.Abstractions;

public class ToolAssemblyService
{
    private readonly ILogger<ToolAssemblyService> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly Dictionary<string, CustomAssemblyLoadContext> _loadContexts = new();
    private readonly Dictionary<string, Assembly> _loadedAssemblies = new();
    private readonly Dictionary<string, Type> _componentTypeCache = new();

    public ToolAssemblyService(ILogger<ToolAssemblyService> logger, IFileSystem fileSystem)
    {
        _logger = logger;
        _fileSystem = fileSystem;
    }

    public Assembly LoadToolAssembly(string dllPath)
    {
        if (_loadedAssemblies.TryGetValue(dllPath, out var assembly))
        {
            _logger.LogInformation("Returning cached assembly for: {DllPath}", dllPath);
            return assembly;
        }

        try
        {
            // Sao chép file DLL vào thư mục tạm hệ thống
            string tempFolder = Path.GetTempPath();
            string tempFilePath = Path.Combine(tempFolder, $"temp_{Guid.NewGuid()}_{Path.GetFileName(dllPath)}");
            _fileSystem.File.Copy(dllPath, tempFilePath, true);

            var loadContext = new CustomAssemblyLoadContext();
            assembly = loadContext.LoadFromAssemblyPath(tempFilePath);
            _loadedAssemblies[dllPath] = assembly;
            _loadContexts[dllPath] = loadContext;
            _logger.LogInformation("Loaded assembly from temporary file: {TempFilePath}", tempFilePath);

            // Xóa file tạm ngay sau khi load
            try
            {
                _fileSystem.File.Delete(tempFilePath);
                _logger.LogInformation("Deleted temporary file: {TempFilePath}", tempFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary file: {TempFilePath}. It will be cleaned up later.", tempFilePath);
            }

            return assembly;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải tool assembly từ {DllPath}", dllPath);
            throw;
        }
    }

    public Assembly LoadToolAssembly(byte[] assemblyData)
    {
        try
        {
            var loadContext = new CustomAssemblyLoadContext();
            var assembly = loadContext.LoadFromStream(new MemoryStream(assemblyData));
            var assemblyName = assembly.GetName().Name ?? Guid.NewGuid().ToString();
            _loadedAssemblies[assemblyName] = assembly;
            _loadContexts[assemblyName] = loadContext;
            _logger.LogInformation("Loaded assembly from binary data: {AssemblyName}", assemblyName);
            return assembly;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải tool assembly từ dữ liệu nhị phân");
            throw;
        }
    }

    public async Task UnloadToolAssemblyAsync(string key)
    {
        if (_loadContexts.TryGetValue(key, out var loadContext))
        {
            try
            {
                loadContext.Unload();
                _logger.LogInformation("Unloaded assembly with key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to unload assembly with key: {Key}", key);
            }
            finally
            {
                _loadContexts.Remove(key);
                _loadedAssemblies.Remove(key);
                var cacheKeysToRemove = _componentTypeCache.Keys
                    .Where(k => k.StartsWith($"{key}:"))
                    .ToList();
                foreach (var cacheKey in cacheKeysToRemove)
                {
                    _componentTypeCache.Remove(cacheKey);
                }

                // Chờ và kiểm tra file gốc có bị khóa không
                bool isFileUnlocked = false;
                for (int i = 0; i < 40; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    await Task.Delay(3000);

                    try
                    {
                        using (var stream = _fileSystem.File.Open(key, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                        {
                            _logger.LogInformation("File {Key} is no longer locked after {Attempts} attempts", key, i + 1);
                            isFileUnlocked = true;
                            break;
                        }
                    }
                    catch (IOException ex)
                    {
                        _logger.LogWarning(ex, "File {Key} is still locked, attempt {Attempt}/40", key, i + 1);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error while checking file lock for {Key}", key);
                        break;
                    }
                }

                if (!isFileUnlocked)
                {
                    _logger.LogWarning("File {Key} is still locked after 40 attempts. It may require manual cleanup or a system restart.", key);
                }
            }
        }
        else
        {
            _logger.LogWarning("No load context found for key: {Key}", key);
        }
    }

    public Type? FindToolComponentType(Assembly assembly, string slug)
    {
        string cacheKey = $"{assembly.FullName}:{slug}";
        if (_componentTypeCache.TryGetValue(cacheKey, out var cachedType))
        {
            _logger.LogInformation("Returning cached component type for cache key: {CacheKey}", cacheKey);
            return cachedType;
        }

        try
        {
            var attributeMarkedTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(ToolComponentAttribute), false)
                             .OfType<ToolComponentAttribute>()
                             .Any(attr => attr.IsMainComponent))
                .ToList();

            if (attributeMarkedTypes.Count > 0)
            {
                var componentType = attributeMarkedTypes.First();
                _componentTypeCache[cacheKey] = componentType;
                _logger.LogInformation("Found component type with ToolComponentAttribute: {ComponentType}", componentType.FullName);
                return componentType;
            }

            var toolTypes = assembly.GetTypes()
                .Where(t => typeof(ITool).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
                .ToList();

            if (toolTypes.Count == 0)
            {
                _logger.LogWarning("Không tìm thấy thành phần Tool nào trong assembly {Assembly}", assembly.FullName);
                return null;
            }

            if (!string.IsNullOrEmpty(slug))
            {
                foreach (var type in toolTypes)
                {
                    try
                    {
                        if (Activator.CreateInstance(type) is ITool tool && 
                            tool.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase))
                        {
                            _componentTypeCache[cacheKey] = type;
                            _logger.LogInformation("Found component type matching slug {Slug}: {ComponentType}", slug, type.FullName);
                            return type;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to create instance of type {Type} while searching for slug {Slug}", type.FullName, slug);
                        continue;
                    }
                }
            }

            if (toolTypes.Count == 1)
            {
                var componentType = toolTypes.First();
                _componentTypeCache[cacheKey] = componentType;
                _logger.LogInformation("Found single component type: {ComponentType}", componentType.FullName);
                return componentType;
            }

            var assemblyName = assembly.GetName().Name;
            var matchingType = toolTypes.FirstOrDefault(t => 
                t.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) ||
                t.Name.Contains(assemblyName?.Replace("Tool", "") ?? "", StringComparison.OrdinalIgnoreCase));
            
            if (matchingType != null)
            {
                _componentTypeCache[cacheKey] = matchingType;
                _logger.LogInformation("Found matching component type: {ComponentType}", matchingType.FullName);
                return matchingType;
            }

            var firstType = toolTypes.First();
            _componentTypeCache[cacheKey] = firstType;
            _logger.LogInformation("Defaulted to first component type: {ComponentType}", firstType.FullName);
            return firstType;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tìm kiếm tool component trong assembly");
            return null;
        }
    }

    public ToolDto ExtractToolMetadata(Assembly assembly)
    {
        try
        {
            var attributeMarkedTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(ToolComponentAttribute), false)
                             .OfType<ToolComponentAttribute>()
                             .Any(attr => attr.IsMainComponent))
                .ToList();

            Type? toolType = null;
            
            if (attributeMarkedTypes.Count > 0)
            {
                toolType = attributeMarkedTypes.First();
                _logger.LogInformation("Extracting metadata using ToolComponentAttribute: {ToolType}", toolType.FullName);
            }
            else
            {
                var toolTypes = assembly.GetTypes()
                    .Where(t => typeof(ITool).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
                    .ToList();

                if (toolTypes.Count == 0)
                {
                    throw new InvalidOperationException("Không tìm thấy Tool component nào trong assembly");
                }

                if (toolTypes.Count == 1)
                {
                    toolType = toolTypes.First();
                    _logger.LogInformation("Extracting metadata from single tool type: {ToolType}", toolType.FullName);
                }
                else
                {
                    var assemblyName = assembly.GetName().Name;
                    toolType = toolTypes.FirstOrDefault(t => 
                        t.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) ||
                        t.Name.Contains(assemblyName?.Replace("Tool", "") ?? "", StringComparison.OrdinalIgnoreCase)) 
                        ?? toolTypes.First();
                    _logger.LogInformation("Extracting metadata from selected tool type: {ToolType}", toolType.FullName);
                }
            }
            
            if (Activator.CreateInstance(toolType) is ITool tool)
            {
                return new ToolDto
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    Slug = tool.Slug,
                    Icon = tool.Icon,
                    IsPremium = tool.RequiresPremium,
                    Group = new ToolGroupDto
                    {
                        Name = tool.Group,
                        Description = $"{tool.Group} Tools"
                    }
                };
            }
            
            throw new InvalidOperationException("Không thể tạo instance của Tool component");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đọc metadata từ tool assembly");
            throw;
        }
    }
}

public class CustomAssemblyLoadContext : AssemblyLoadContext
{
    public CustomAssemblyLoadContext() : base(isCollectible: true)
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return null;
    }
}