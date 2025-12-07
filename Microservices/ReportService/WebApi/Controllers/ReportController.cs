using Core.DTOs;
using Core.DTOs.AI;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ReportController(IReportService reportService) : ControllerBase
{
    //[HttpPost("teams/{teamId}")]
    [HttpPost("teams")]
    public async Task<ActionResult<AnalysisResponseDto>> CreateReport(string userRequest)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await reportService.CreateReport(userRequest);
        if (result == null)
            return NotFound("there is no team with this id");
        return Ok(result);
    }
    [HttpGet("teams/{teamId}")]
    public async Task<ActionResult> GetReport([FromRoute] string teamId, [FromQuery] string taskId)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await reportService.GetReport(teamId, taskId);
        if (result == null)
            return NotFound("there is no team with this id");
        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<AiReportResponseDto>> GetReportById(string id)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await reportService.GetReportById(id);
        if (result == null)
            return NotFound("there is no report with this id");
        return Ok(result);
    }
    [HttpDelete("{id}")]
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
    [HttpPost("dataAssing/{companyId}")]
    public async Task<ActionResult<string>> AssignDataToCompany([FromRoute] string companyId)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await reportService.AssignCompanyData(companyId);
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
}
