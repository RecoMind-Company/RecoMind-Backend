using Core.DTOs;
using Core.DTOs.AI;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController(IReportService reportService) : ControllerBase
{
    [HttpGet("teams/user")]
    public async Task<ActionResult<TeamToReturnDto>> GetUserData()
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UserId == null)
            return NotFound("There is no user!");
        var result = await reportService.GetUserData(UserId);
        if (result == null)
            return NotFound("there is no team for this user");
        return Ok(result);
    }
    [HttpPost("teams/create")]
    public async Task<ActionResult<AnalysisResponseDto>> CreateReport(AnalysisRequestDto analysisRequest)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await reportService.CreateReportByAi(analysisRequest);
        if (result == null)
            return NotFound("Error while creating the report!");
        return Ok(result);
    }
    [HttpPost("teams/add")]
    public async Task<ActionResult<AiReportResponseDto>> AddReport(GetReportFromAiDto reportFromAiDto)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return NotFound("There is no user!");
        reportFromAiDto.UserId = userId;
        var result = await reportService.GetReportFromAi(reportFromAiDto);
        if (!result.IsSuccess)
            return NotFound(result.message);
        return Ok(result);
    }
    [HttpGet("get/{id}")]
    public async Task<ActionResult<DividedReportDto>> GetReportById(string id)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await reportService.GetReportById(id);
        if (result.ErrorMessage is not null)
            return NotFound(result.ErrorMessage);
        return Ok(result);
    }
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<string>> DeleteReport(string id)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await reportService.DeleteReport(id);
        if (string.IsNullOrEmpty(result))
            return NotFound("there is no report with this id");
        return Ok(result);
    }

    [HttpGet("all/{teamId}")]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetReportsByTeamId([FromRoute] string teamId, [FromQuery] int limit)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await reportService.GetAllReportsByTeamId(teamId, limit);
        if (result == null || !result.Any())
            return NotFound("there is no reports for this team");
        return Ok(result);
    }

    // NOT REPORT ENDPOINTS
    [HttpGet("dataAssing")]
    public async Task<ActionResult<string>> AssignDataToCompany()
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var userCompanyId = User.FindFirstValue("CompanyId");
        var result = await reportService.AssignCompanyData(userCompanyId);
        if (string.IsNullOrEmpty(result))
            return NotFound("there is no company with this id");
        return Ok(result);
    }
    [HttpGet("dataAssignStatus/{taskId}")]
    public async Task<ActionResult<string>> GetDataAssignResult([FromRoute] string taskId)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await reportService.GetAssignCompanyDataStatus(taskId);
        if (result == null)
            return NotFound("there is no task with this id");
        return Ok(result);
    }
    [HttpPost("test")]
    public async Task<ActionResult<ReportDto>> AddTestReport(TestDto testDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        testDto.UserId = userId;
        var response = await reportService.CreateTestReport(testDto);
        return response;
    }
}
