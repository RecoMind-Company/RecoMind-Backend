using Core.DTOs.AI.ValidationReport;
using Core.DTOs.AI.ValidationReport.AIResult;
using Core.DTOs.ValidationReport;
using Core.Interfaces;
using Core.Models;
using Core.Service.Interface;
using Core.Service.Interface.AI;
using Infrastructure.GrpcClients.Team;

namespace Core.Service;

public class ValidationReportService(IValidationReportGeneratorService reportGeneratorService,
                                     IUnitOfWork<ValidationReport> unitOfWork,
                                     ITeamGrpcClient teamGrpcClient) : IValidationReportService
{
    public async Task<Result<AIValidationReportResponseDto>> RequestValidationReport(UserValidationReportRequestDto requestDto)
    {
        //var TeamId = "0dc1400d-a758-424b-80fb-a8ff89078522"; // FOR TEST ------------------------------

        var checkTeamId = await teamGrpcClient.GetTeamNameById(requestDto.UserId);  //Check if the user is part of a team and get the team id
        if (!checkTeamId.IsSuccess)
            return Result<AIValidationReportResponseDto>.Failure(checkTeamId.Error);

        var aiRequest = new AIValidationReportRequestDto
        {
            CompanyId = requestDto.CompanyId,
            TeamId = checkTeamId.Value,
            UserRequest = requestDto.UserRequest
        };
        var response = await reportGeneratorService.GenerateValidationReport(aiRequest);
        return response;
    }
    public async Task<Result<ValidationReportDto>> GetValidationReport(string taskId)
    {
        var response = await reportGeneratorService.GetValidationReport(taskId);

        if (!response.IsSuccess)
            return Result<ValidationReportDto>.Failure(response.Error);

        var validationReport = response.Value.Result.ValidationReport;
        return Result<ValidationReportDto>.Success(validationReport);
    }
}
