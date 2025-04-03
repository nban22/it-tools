using it_tools.Data.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace it_tools.Data.Services;

public interface IToolService
{
    Task<ToolDto?> UploadToolAsync(IBrowserFile zipFile);
    Task<bool> DeleteToolAsync(string toolId);
}