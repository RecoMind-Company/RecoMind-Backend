using Core.DTOs.AI;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Infrastructure.AI;
// WILL BE REFACTORED LATER (to read the 422 json response body and return it.)
public class GenerateReportService(HttpClient httpClient, ILogger<GenerateReportService> logger) : IGenerateReportService
{
    public async Task<AnalysisResponseDto> GenerateReport(AnalysisRequestDto request)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("run_analysis", request);
            response.EnsureSuccessStatusCode();
            var analysisResponse = await response.Content.ReadFromJsonAsync<AnalysisResponseDto>();
            return analysisResponse!;
        }
        catch (HttpRequestException ex)
        {
            int statusCode = (int)(ex.StatusCode ?? 0);
            if (statusCode == StatusCodes.Status422UnprocessableEntity)
            {
                logger.LogWarning("Failed to create report for {0} team.", request.TeamName);
                return null;
            }
            else
            {
                logger.LogError("HTTP Request Error while creating a report for {0} team: {1}", request.TeamName, ex.Message);
                return null;
            }
        }
    }

    public async Task<TaskStatusResponseDto> GetStatus(string taskId)
    {
        try
        {
            var response = await httpClient.GetAsync($"get_status/{taskId}");
            response.EnsureSuccessStatusCode();
            var status = await response.Content.ReadFromJsonAsync<TaskStatusResponseDto>();
            return status!;
        }
        catch (HttpRequestException ex)
        {
            int statusCode = (int)(ex.StatusCode ?? 0);
            if (statusCode == StatusCodes.Status422UnprocessableEntity)
            {
                logger.LogWarning("Failed to get status for Task with $ID {0}.", taskId);
                return null;
            }
            else
            {
                logger.LogError("HTTP Request Error while getting status for Task with ID {0}: {1}", taskId, ex.Message);
                return null;
            }
        }
    }
}
