// // ToolAssemblyService.cs
// using System.Reflection;
// using it_tools.Data.DTOs;
// using it_tools.ToolDevelopment.Attributes;
// using it_tools.ToolDevelopment.Interfaces;

// public class ToolAssemblyService
// {
//     private readonly ILogger<ToolAssemblyService> _logger;
//     private readonly Dictionary<string, Assembly> _loadedAssemblies = new();
//     private readonly Dictionary<string, Type> _componentTypeCache = new();

//     public ToolAssemblyService(ILogger<ToolAssemblyService> logger)
//     {
//         _logger = logger;
//     }

//     public Assembly LoadToolAssembly(string dllPath)
//     {
//         if (_loadedAssemblies.TryGetValue(dllPath, out var assembly))
//         {
//             return assembly;
//         }

//         try
//         {
//             assembly = Assembly.LoadFrom(dllPath);
//             _loadedAssemblies[dllPath] = assembly;
//             return assembly;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Không thể tải tool assembly từ {DllPath}", dllPath);
//             throw;
//         }
//     }

//     public Assembly LoadToolAssembly(byte[] assemblyData)
//     {
//         try
//         {
//             var assembly = Assembly.Load(assemblyData);
//             return assembly;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Không thể tải tool assembly từ dữ liệu nhị phân");
//             throw;
//         }
//     }

//     public Type? FindToolComponentType(Assembly assembly, string slug)
//     {
//         // Kiểm tra cache trước
//         string cacheKey = $"{assembly.FullName}:{slug}";
//         if (_componentTypeCache.TryGetValue(cacheKey, out var cachedType))
//         {
//             return cachedType;
//         }

//         try
//         {
//             // 1. Tìm kiếm các lớp được đánh dấu với ToolComponentAttribute
//             var attributeMarkedTypes = assembly.GetTypes()
//                 .Where(t => t.GetCustomAttributes(typeof(ToolComponentAttribute), false)
//                              .OfType<ToolComponentAttribute>()
//                              .Any(attr => attr.IsMainComponent))
//                 .ToList();

//             if (attributeMarkedTypes.Count > 0)
//             {
//                 var componentType = attributeMarkedTypes.First();
//                 _componentTypeCache[cacheKey] = componentType;
//                 return componentType;
//             }

//             // 2. Tìm kiếm các lớp implement ITool
//             var toolTypes = assembly.GetTypes()
//                 .Where(t => typeof(ITool).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
//                 .ToList();

//             if (toolTypes.Count == 0)
//             {
//                 _logger.LogWarning("Không tìm thấy thành phần Tool nào trong assembly {Assembly}", assembly.FullName);
//                 return null;
//             }

//             // 3. Thử khớp với slug
//             if (!string.IsNullOrEmpty(slug))
//             {
//                 // Tạo instance của mỗi type để kiểm tra slug
//                 foreach (var type in toolTypes)
//                 {
//                     try
//                     {
//                         if (Activator.CreateInstance(type) is ITool tool && 
//                             tool.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase))
//                         {
//                             _componentTypeCache[cacheKey] = type;
//                             return type;
//                         }
//                     }
//                     catch 
//                     {
//                         // Bỏ qua lỗi khi tạo instance
//                         continue;
//                     }
//                 }
//             }

//             // 4. Nếu chỉ có 1 loại, trả về loại đó
//             if (toolTypes.Count == 1)
//             {
//                 var componentType = toolTypes.First();
//                 _componentTypeCache[cacheKey] = componentType;
//                 return componentType;
//             }

//             // 5. Tìm loại nào có tên trùng với tên Assembly
//             var assemblyName = assembly.GetName().Name;
//             var matchingType = toolTypes.FirstOrDefault(t => 
//                 t.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) ||
//                 t.Name.Contains(assemblyName?.Replace("Tool", "") ?? "", StringComparison.OrdinalIgnoreCase));
            
//             if (matchingType != null)
//             {
//                 _componentTypeCache[cacheKey] = matchingType;
//                 return matchingType;
//             }

//             // 6. Phương án cuối: trả về tool đầu tiên
//             var firstType = toolTypes.First();
//             _componentTypeCache[cacheKey] = firstType;
//             return firstType;
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Lỗi khi tìm kiếm tool component trong assembly");
//             return null;
//         }
//     }

//     public ToolDto ExtractToolMetadata(Assembly assembly)
//     {
//         try
//         {
//             // 1. Tìm kiếm các lớp được đánh dấu với ToolComponentAttribute
//             var attributeMarkedTypes = assembly.GetTypes()
//                 .Where(t => t.GetCustomAttributes(typeof(ToolComponentAttribute), false)
//                              .OfType<ToolComponentAttribute>()
//                              .Any(attr => attr.IsMainComponent))
//                 .ToList();

//             Type? toolType = null;
            
//             if (attributeMarkedTypes.Count > 0)
//             {
//                 toolType = attributeMarkedTypes.First();
//             }
//             else
//             {
//                 // 2. Tìm các lớp implement ITool
//                 var toolTypes = assembly.GetTypes()
//                     .Where(t => typeof(ITool).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass)
//                     .ToList();

//                 if (toolTypes.Count == 0)
//                 {
//                     throw new InvalidOperationException("Không tìm thấy Tool component nào trong assembly");
//                 }

//                 // Lấy type đầu tiên nếu chỉ có một
//                 if (toolTypes.Count == 1)
//                 {
//                     toolType = toolTypes.First();
//                 }
//                 else
//                 {
//                     // Tìm type có tên trùng với tên assembly
//                     var assemblyName = assembly.GetName().Name;
//                     toolType = toolTypes.FirstOrDefault(t => 
//                         t.Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) ||
//                         t.Name.Contains(assemblyName?.Replace("Tool", "") ?? "", StringComparison.OrdinalIgnoreCase)) 
//                         ?? toolTypes.First();
//                 }
//             }
            
//             // Tạo instance để lấy metadata
//             if (Activator.CreateInstance(toolType) is ITool tool)
//             {
//                 return new ToolDto
//                 {
//                     Name = tool.Name,
//                     Description = tool.Description,
//                     Slug = tool.Slug,
//                     Icon = tool.Icon,
//                     IsPremium = tool.RequiresPremium,
//                     Group = new ToolGroupDto
//                     {
//                         Name = tool.Group,
//                         Description = $"{tool.Group} Tools"
//                     }
//                 };
//             }
            
//             throw new InvalidOperationException("Không thể tạo instance của Tool component");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Lỗi khi đọc metadata từ tool assembly");
//             throw;
//         }
//     }
// }

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
        // Kiểm tra xem assembly đã được tải chưa
        if (_loadedAssemblies.TryGetValue(dllPath, out var assembly))
        {
            return assembly;
        }

        try
        {
            // Sử dụng AssemblyLoadContext để tải assembly
            var loadContext = new CustomAssemblyLoadContext();
            assembly = loadContext.LoadFromAssemblyPath(dllPath);
            _loadedAssemblies[dllPath] = assembly;
            _loadContexts[dllPath] = loadContext; // Lưu context để unload sau
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
            // Sử dụng AssemblyLoadContext để tải assembly từ dữ liệu nhị phân
            var loadContext = new CustomAssemblyLoadContext();
            var assembly = loadContext.LoadFromStream(new MemoryStream(assemblyData));
            var assemblyName = assembly.GetName().Name ?? Guid.NewGuid().ToString();
            _loadedAssemblies[assemblyName] = assembly;
            _loadContexts[assemblyName] = loadContext; // Lưu context để unload sau
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
                // Xóa cache liên quan
                var cacheKeysToRemove = _componentTypeCache.Keys
                    .Where(k => k.StartsWith($"{key}:"))
                    .ToList();
                foreach (var cacheKey in cacheKeysToRemove)
                {
                    _componentTypeCache.Remove(cacheKey);
                }
                // Đợi GC giải phóng tài nguyên
                GC.Collect();
                GC.WaitForPendingFinalizers();
                _logger.LogInformation("Unloaded assembly with key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to unload assembly with key: {Key}", key);
            }
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

// Custom AssemblyLoadContext để hỗ trợ unload
public class CustomAssemblyLoadContext : AssemblyLoadContext
{
    public CustomAssemblyLoadContext() : base(isCollectible: true)
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return null; // Không tải assembly phụ tự động
    }
}