using Core.DTOs.AI;
using Core.Exceptions;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.AI;

public class DataAssignService(HttpClient httpClient, ILogger<DataAssignService> logger) : IDataAssignService
{
    public async Task<string> DataAssign(string companyId)
    {
        /*
        {
        "company_id": "string"
        },
        {
          "task_id": "cd2fa71d-d328-4110-bc0c-608f676a0cf3",
          "status": "PENDING",
          "message": "Ingestion task has been submitted.",
          "company_id": "RecoMind_Company"
        }
         */
        var requestDto = new DataAssignRequestDto { CompanyId = companyId };
        var response = await httpClient.PostAsJsonAsync("start-pipeline", requestDto);
        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var errors = JsonSerializer.Deserialize<ReportValidationError>(responseContent);
            logger.LogWarning("Failed to Data assign for company with Id: {0}.", requestDto.CompanyId);
            throw new UnprocessableEntityException("validation errors occurred", errors!);
        }
        response.EnsureSuccessStatusCode();
        var DataAssignResponse = await response.Content.ReadFromJsonAsync<DataAssignResponseDto>();
        return DataAssignResponse!.TaskId;
    }

    public async Task<string> DataAssignResult(string taskId)
    {
        // task_id
        // send get request to check status 
        // cheack status code if == 422
        // Ensure Success Status code
        // return status

        /*
        {
          "task_id": "cd2fa71d-d328-4110-bc0c-608f676a0cf3",
          "status": "SUCCESS",
          "result": {
            "message": "Pipeline completed successfully",
            "tables_processed": 71,
            "tables_with_teams": 68,
            "execution_time_seconds": 145.2
            }
        }
         */
        var response = await httpClient.GetAsync($"task-status/{taskId}");
        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var errors = JsonSerializer.Deserialize<ReportValidationError>(responseContent);
            logger.LogWarning("Failed to get status for Task with ID {0}.", taskId);
            throw new UnprocessableEntityException("validation errors occurred", errors!);
        }

        response.EnsureSuccessStatusCode();
        var status = await response.Content.ReadFromJsonAsync<DataAssignStatusDto>();

        if (status!.Status == Status.FAILURE)
        {
            logger.LogError("Task with ID {0} failed during processing.", taskId);
            throw new Exception($"Task with ID {taskId} failed during processing.");
        }
        return status!.Status;
    }
}
