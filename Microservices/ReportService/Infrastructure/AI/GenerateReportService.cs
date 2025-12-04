using Core.DTOs.AI;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.AI;
// WILL BE REFACTORED LATER (to read the 422 json response body and return it.)
public class GenerateReportService(HttpClient httpClient, ILogger<GenerateReportService> logger) : IGenerateReportService
{
    public async Task<AnalysisResponseDto> GenerateReport(AnalysisRequestDto request)
    {
        var response = await httpClient.PostAsJsonAsync("run_analysis", request);

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var errors = JsonSerializer.Deserialize<ReportValidationError>(responseContent);
            logger.LogWarning("Failed to create report for {0} team.", request.TeamName);
            throw new UnprocessableEntityException("validation errors occurred", errors!);
        }

        response.EnsureSuccessStatusCode();

        var analysisResponse = await response.Content.ReadFromJsonAsync<AnalysisResponseDto>();
        return analysisResponse!;
    }

    public async Task<TaskStatusResponseDto> GetStatus(string taskId)
    {
        var response = await httpClient.GetAsync($"get_status/{taskId}");
        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var errors = JsonSerializer.Deserialize<ReportValidationError>(responseContent);
            logger.LogWarning("Failed to get status for Task with ID {0}.", taskId);
            throw new UnprocessableEntityException("validation errors occurred", errors!);
        }
        response.EnsureSuccessStatusCode();
        var status = await response.Content.ReadFromJsonAsync<TaskStatusResponseDto>();
        if (status!.Status == Status.FAILURE)
        {
            logger.LogError("Task with ID {0} failed during processing.", taskId);
            throw new Exception($"Task with ID {taskId} failed during processing.");
        }
        return status!;
    }
}
