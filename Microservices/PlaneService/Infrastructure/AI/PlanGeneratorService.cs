using Core.DTOs;
using Core.DTOs.AI;
using Core.Models;
using Core.Service.Interface.AI;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Infrastructure.AI;

public class PlanGeneratorService(HttpClient httpClient, IConfiguration configuration) : IPlanGeneratorService
{
    private readonly string GeneratePlanRoute = configuration.GetValue<string>("AI:ProductionRoute");
    private readonly string GetPlanRoute = configuration.GetValue<string>("AI:GetPlanRoute");
    public async Task<Result<RequestCustomPlanResponseDto>> GeneratePlan(AIRequestDto RequestDto)
    {
        var response = await httpClient.PostAsJsonAsync(GeneratePlanRoute, RequestDto);
        try
        {
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<RequestCustomPlanResponseDto>();
            return Result<RequestCustomPlanResponseDto>.Success(result!);
        }
        catch (Exception)
        {
            return Result<RequestCustomPlanResponseDto>.Failure($"Failed to generate plan");
        }
    }

    public async Task<Result<AIPlanDto>> GetPlanResult(string taskId)
    {
        var response = httpClient.GetAsync($"{GetPlanRoute}/{taskId}");
        try
        {
            response.Result.EnsureSuccessStatusCode();
            var result = await response.Result.Content.ReadFromJsonAsync<FullAIResponseDto>();

            if (result.status.ToLower() == "pending")
                return Result<AIPlanDto>.Failure($"Plan generation is still pending.");

            if (result.status.ToLower() == "progress" && result.status.ToLower() == "started")
                return Result<AIPlanDto>.Failure($"Plan generation is still in progress.");

            if (result.status.ToLower() == "success")
                return Result<AIPlanDto>.Success(result.result!);


            var staticResponse = StaticAiResponse.StaticResponse;
            return Result<AIPlanDto>.Success(staticResponse);
        }
        catch (Exception)
        {
            var staticResponse = StaticAiResponse.StaticResponse;
            return Result<AIPlanDto>.Success(staticResponse);
        }
    }
}
