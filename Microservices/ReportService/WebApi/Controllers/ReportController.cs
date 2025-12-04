using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController(IReportService reportService) : ControllerBase
{
    [HttpGet("teams/{teamId}")]
    public async Task<ActionResult> GetReport([FromRoute] string teamId, [FromQuery] GetReportDto getReportDto)
    {
        var errors = ModelState.Values.SelectMany(e => e.Errors);
        if (!ModelState.IsValid)
            return BadRequest(errors);
        getReportDto.TeamId = teamId;
        var result = await reportService.GetReport(getReportDto);
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
