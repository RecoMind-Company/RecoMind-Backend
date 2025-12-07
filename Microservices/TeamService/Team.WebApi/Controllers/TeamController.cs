using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using System.Security.Claims;
using Team.Core.DTOs;
using Team.Core.Exceptions;
using Team.Core.Interfaces;
using Team.Core.Services;

namespace Team.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _service;
        private readonly ILogger<TeamController> _logger;
        public TeamController(ITeamService service, ILogger<TeamController> logger)
        {
            _service = service;
            _logger = logger;
        }


        [HttpGet("{teamId}/company/{companyId}")]
        public async Task<IActionResult> GetTeam(string teamId, string companyId)
        {
            try
            {
                var team = await _service
                    .GetTeamAsync(teamId, companyId);
                return Ok(team);
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ForbiddenException ex) { return StatusCode(403, new { message = ex.Message }); }
        }

        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetTeamsForCompany(string companyId)
        {
            var teams = await _service.GetTeamsForCompanyAsync(companyId);
            return Ok(teams);
        }

        [HttpGet("company/{companyId}/for-ai-model")]
        public async Task<IActionResult> GetTeamsForAI(string companyId)
        {
            var teams = await _service.GetTeamsForAiAsync(companyId);
            return Ok(teams);
        }


        [HttpPost("company/{companyId}")]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto dto, string companyId)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var created = await _service.CreateTeamAsync(companyId, dto);
                return Ok(created);
            }
            catch (ConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{teamId}/company/{companyId}")]
        public async Task<IActionResult> UpdateTeam(string teamId, [FromBody] UpdateTeamDto dto, string companyId)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            try
            {
                var updated = await _service.UpdateTeamAsync(teamId, companyId, dto);
                return Ok(updated);
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ConflictException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpDelete("{teamId}/company/{companyId}")]
        public async Task<IActionResult> DeleteTeam(string teamId, string companyId)
        {
            try
            {
                await _service.DeleteTeamAsync(teamId, companyId);
                return NoContent();
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost("{teamId}/company/{companyId}/employees")]
        public async Task<IActionResult> AddEmployee(string teamId, [FromBody] AddEmployeeDto emp, string companyId)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            try
            {
                var added = await _service.AddEmployeeAsync(teamId, companyId, emp.EmployeeId);
                if (!added) return BadRequest(new { message = "Employee already in team" });
                
                return Ok(new { message = "Employee added" });
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }


        [HttpDelete("{teamId}/company/{companyId}/employees/{employeeId}")]
        public async Task<IActionResult> RemoveEmployee(string teamId, string companyId, string employeeId)
        {
            try
            {
                var removed = await _service.RemoveEmployeeAsync(teamId, companyId, employeeId);

                if (!removed) return BadRequest(new { message = "Employee not found in team" });

                return Ok(new { message = "Employee removed" });
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}
