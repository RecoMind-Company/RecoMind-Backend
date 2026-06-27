using Core.Models;

namespace Core.Service.Interface;

public interface IModuleService
{
    Task<Module?> GetModuleByIdAsync(string moduleId);
}
