using System.Reflection;
using System.Runtime.Loader;
using it_tools.Data.DTOs;
using it_tools.ToolDevelopment.Attributes;
using it_tools.ToolDevelopment.Interfaces;

public class ToolAssemblyService
{
    private readonly ILogger<ToolAssemblyService> _logger;
    private readonly Dictionary<string, CustomAssemblyLoadContext> _loadContexts = new();
    private readonly Dictionary<string, Assembly> _loadedAssemblies = new();
    private readonly Dictionary<string, Type> _componentTypeCache = new();

    public ToolAssemblyService(ILogger<ToolAssemblyService> logger)
    {
        _logger = logger;
    }

    public Assembly LoadToolAssembly(string dllPath)
    {
        if (_loadedAssemblies.TryGetValue(dllPath, out var assembly))
        {
            return assembly;
        }

        try
        {
            var loadContext = new CustomAssemblyLoadContext();
            assembly = loadContext.LoadFromAssemblyPath(dllPath);
            _loadedAssemblies[dllPath] = assembly;
            _loadContexts[dllPath] = loadContext;
            _logger.LogInformation("Loaded assembly from: {DllPath}", dllPath);
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

    public void UnloadToolAssembly(string key)
    {
        if (_loadContexts.TryGetValue(key, out var loadContext))
        {
            try
            {
                loadContext.Unload();
                _loadContexts.Remove(key);
                _loadedAssemblies.Remove(key);
                var cacheKeysToRemove = _componentTypeCache.Keys
                    .Where(k => k.StartsWith($"{key}:"))
                    .ToList();
                foreach (var cacheKey in cacheKeysToRemove)
                {
                    _componentTypeCache.Remove(cacheKey);
                }
                for (int i = 0; i < 5; i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Thread.Sleep(500);
                }
                _logger.LogInformation("Unloaded assembly with key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to unload assembly with key: {Key}", key);
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
                            return type;
                        }
                    }
                    catch 
                    {
                        continue;
                    }
                }
            }

            if (toolTypes.Count == 1)
            {
                var componentType = toolTypes.First();
                _componentTypeCache[cacheKey] = componentType;
                return componentType;
            }

            var assemblyName = assembly.GetName().Name;
            var matchingType = toolTypes.FirstOrDefault(t => 
                t.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) ||
                t.Name.Contains(assemblyName?.Replace("Tool", "") ?? "", StringComparison.OrdinalIgnoreCase));
            
            if (matchingType != null)
            {
                _componentTypeCache[cacheKey] = matchingType;
                return matchingType;
            }

            var firstType = toolTypes.First();
            _componentTypeCache[cacheKey] = firstType;
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
                }
                else
                {
                    var assemblyName = assembly.GetName().Name;
                    toolType = toolTypes.FirstOrDefault(t => 
                        t.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) ||
                        t.Name.Contains(assemblyName?.Replace("Tool", "") ?? "", StringComparison.OrdinalIgnoreCase)) 
                        ?? toolTypes.First();
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