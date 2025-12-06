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
    public async Task<ActionResult<AnalysisResponseDto>> CreateReport()
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        var result = await reportService.CreateReport();
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
}
