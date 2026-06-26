using Core.DTOs.AI;
using Core.Models;
using Core.Service.Interface.AI;
using System.Net.Http.Json;

namespace Infrastructure.AI;

public class PlanGeneratorService(HttpClient httpClient) : IPlanGeneratorService
{
    public async Task<Result<AIResultDto>> GeneratePlan(AIRequestDto RequestDto)
    {
        var response = await httpClient.PostAsJsonAsync("ROUTE_TO_AI_SERVICE", RequestDto);
        try
        {
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<AIResultDto>();
            return Result<AIResultDto>.Success(result!);
        }
        catch (Exception)
        {
            return Result<AIResultDto>.Failure($"Failed to generate plan because {response.Content}");
        }
    }
}
