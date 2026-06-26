using Core.Service.Interface;
using Grpc.Core;
using System.Threading.Tasks;
using webApi.Grpc;

namespace WebApi.GrpcServer;

public class ModuleServiceImplementation(IModuleService moduleService) : ModuleServiceGrpc.ModuleServiceGrpcBase
{
    public override async Task<ModuleResponse> GetModule(ModuleRequest request, ServerCallContext context)
    {
        var module = await moduleService.GetModuleByIdAsync(request.ModuleId);
        if (module == null)
        {
            return new ModuleResponse { IsExist = false };
        }

        return new ModuleResponse
        {
            IsExist = true,
            ModuleId = module.Id,
            CompanyId = module.Plan.Company_Id,
            TeamId = module.Plan.Team_Id,
            OwnerId = module.Plan.Owner_Id
        };
    }
}
