using Core.Interfaces;
using Core.Models;
using Core.Service.Interface;

namespace Core.Service;

public class ModuleService(IUnitOfWork<Module> unitOfWork) : IModuleService
{
    public async Task<Module?> GetModuleByIdAsync(string moduleId)
    {
        var module = await unitOfWork.Entity.Find(x => x.Id == moduleId, p => p.Plan);
        return module;
    }
}
