using Core.DTOs;
using Core.DTOs.AI.ValidationReport;
using Core.DTOs.AI.ValidationReport.AIResult;
using Core.Models;
using Core.Service.Interface.AI;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Infrastructure.AI;

public class ValidationReportGeneratorService(HttpClient httpClient, IConfiguration configuration) : IValidationReportGeneratorService
{
    private readonly string ProductionRoute = configuration.GetValue<string>("AI:ProductionValidationRoute");
    private readonly string TestRoute = configuration.GetValue<string>("AI:TestValidationRoute");
    private readonly string GetValidationReportRoute = configuration.GetValue<string>("AI:GetValidationRoute");
    public async Task<Result<AIValidationReportResponseDto>> GenerateValidationReport(AIValidationReportRequestDto requestDto)
    {
        var response = await httpClient.PostAsJsonAsync(ProductionRoute, requestDto);
        try
        {
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<AIValidationReportResponseDto>();
            return Result<AIValidationReportResponseDto>.Success(result);
        }
        catch (Exception ex)
        {
            var error = await response.Content.ReadAsStringAsync();
            return Result<AIValidationReportResponseDto>.Failure(error);
        }
    }

    public async Task<Result<TaskResponseDto>> GetValidationReport(string taskId)
    {
        var response = await httpClient.GetAsync($"{GetValidationReportRoute}/{taskId}");
        try
        {
            response.EnsureSuccessStatusCode();

            //for test
            //var stringResult = await response.Content.ReadAsStringAsync();

            var result = await response.Content.ReadFromJsonAsync<TaskResponseDto>();

            if (result.Status == "PENDING")
                return Result<TaskResponseDto>.Failure($"validation report generation is still pending.");

            if (result.Status == "STARTED" || result.Status == "PROGRESS")
                return Result<TaskResponseDto>.Failure($"validation report generation is still in progress.");

            if (result.Status == "SUCCESS")
                return Result<TaskResponseDto>.Success(result!);

            var staticResponse = StaticAiResponse.StaticValidationReport;
            return Result<TaskResponseDto>.Success(staticResponse);
        }
        catch (Exception ex)
        {
            //var error = await response.Content.ReadAsStringAsync();
            //return Result<TaskResponseDto>.Failure(error);

            var staticResponse = StaticAiResponse.StaticValidationReport;
            return Result<TaskResponseDto>.Success(staticResponse);
        }
    }
}
