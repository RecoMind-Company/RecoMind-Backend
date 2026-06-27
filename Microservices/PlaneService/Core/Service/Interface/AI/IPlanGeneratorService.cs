using Core.DTOs.AI;
using Core.Models;

namespace Core.Service.Interface.AI;

public interface IPlanGeneratorService
{
    Task<Result<AIPlanDto>> GeneratePlan(AIRequestDto RequestDto);
}
