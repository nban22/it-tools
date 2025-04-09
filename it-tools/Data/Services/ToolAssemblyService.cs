// ToolAssemblyService.cs
using System.Reflection;
using it_tools.Data.DTOs;
using it_tools.ToolDevelopment.Attributes;
using it_tools.ToolDevelopment.Interfaces;

public class ToolAssemblyService
{
    private readonly ILogger<ToolAssemblyService> _logger;
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
            assembly = Assembly.LoadFrom(dllPath);
            _loadedAssemblies[dllPath] = assembly;
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
            var assembly = Assembly.Load(assemblyData);
            return assembly;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải tool assembly từ dữ liệu nhị phân");
            throw;
        }
    }

    public Type? FindToolComponentType(Assembly assembly, string slug)
    {
        // Kiểm tra cache trước
        string cacheKey = $"{assembly.FullName}:{slug}";
        if (_componentTypeCache.TryGetValue(cacheKey, out var cachedType))
        {
            return cachedType;
        }

        try
        {
            // 1. Tìm kiếm các lớp được đánh dấu với ToolComponentAttribute
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

            // 2. Tìm kiếm các lớp implement ITool
            var toolTypes = assembly.GetTypes()
                .Where(t => typeof(ITool).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
                .ToList();

            if (toolTypes.Count == 0)
            {
                _logger.LogWarning("Không tìm thấy thành phần Tool nào trong assembly {Assembly}", assembly.FullName);
                return null;
            }

            // 3. Thử khớp với slug
            if (!string.IsNullOrEmpty(slug))
            {
                // Tạo instance của mỗi type để kiểm tra slug
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
                        // Bỏ qua lỗi khi tạo instance
                        continue;
                    }
                }
            }

            // 4. Nếu chỉ có 1 loại, trả về loại đó
            if (toolTypes.Count == 1)
            {
                var componentType = toolTypes.First();
                _componentTypeCache[cacheKey] = componentType;
                return componentType;
            }

            // 5. Tìm loại nào có tên trùng với tên Assembly
            var assemblyName = assembly.GetName().Name;
            var matchingType = toolTypes.FirstOrDefault(t => 
                t.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) ||
                t.Name.Contains(assemblyName?.Replace("Tool", "") ?? "", StringComparison.OrdinalIgnoreCase));
            
            if (matchingType != null)
            {
                _componentTypeCache[cacheKey] = matchingType;
                return matchingType;
            }

            // 6. Phương án cuối: trả về tool đầu tiên
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
            // 1. Tìm kiếm các lớp được đánh dấu với ToolComponentAttribute
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
                // 2. Tìm các lớp implement ITool
                var toolTypes = assembly.GetTypes()
                    .Where(t => typeof(ITool).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
                    .ToList();

                if (toolTypes.Count == 0)
                {
                    throw new InvalidOperationException("Không tìm thấy Tool component nào trong assembly");
                }

                // Lấy type đầu tiên nếu chỉ có một
                if (toolTypes.Count == 1)
                {
                    toolType = toolTypes.First();
                }
                else
                {
                    // Tìm type có tên trùng với tên assembly
                    var assemblyName = assembly.GetName().Name;
                    toolType = toolTypes.FirstOrDefault(t => 
                        t.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) ||
                        t.Name.Contains(assemblyName?.Replace("Tool", "") ?? "", StringComparison.OrdinalIgnoreCase)) 
                        ?? toolTypes.First();
                }
            }
            
            // Tạo instance để lấy metadata
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