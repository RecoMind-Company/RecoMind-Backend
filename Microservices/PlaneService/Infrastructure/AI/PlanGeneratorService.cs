using Core.DTOs.AI;
using Core.Models;
using Core.Service.Interface.AI;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Infrastructure.AI;

public class PlanGeneratorService(HttpClient httpClient, IConfiguration configuration) : IPlanGeneratorService
{
    private readonly string AiRoute = configuration.GetValue<string>("AI:ProductionRoute");
    public async Task<Result<AIPlanDto>> GeneratePlan(AIRequestDto RequestDto)
    {
        var response = await httpClient.PostAsJsonAsync(AiRoute, RequestDto);
        try
        {
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<AIPlanDto>();
            return Result<AIPlanDto>.Success(result!);
        }
        catch (Exception)
        {
            return Result<AIPlanDto>.Failure($"Failed to generate plan");
        }
    }
}
