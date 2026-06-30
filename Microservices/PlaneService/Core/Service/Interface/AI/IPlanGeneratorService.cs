using Core.DTOs.AI;
using Core.Models;

namespace Core.Service.Interface.AI;

public interface IPlanGeneratorService
{
    Task<Result<RequestCustomPlanResponseDto>> GeneratePlan(AIRequestDto RequestDto);
    Task<Result<AIPlanDto>> GetPlanResult(string taskId);
}
