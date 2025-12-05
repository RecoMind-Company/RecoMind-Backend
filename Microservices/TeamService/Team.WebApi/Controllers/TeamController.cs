using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private string CompanyId => GetCompanyIdFromClaims();

        public TeamController(ITeamService service, ILogger<TeamController> logger)
        {
            _service = service;
            _logger = logger;
        }


        [HttpGet("{teamId}")]
        public async Task<IActionResult> GetTeam(string teamId)
        {
            try
            {
                var team = await _service
                    .GetTeamAsync(teamId, CompanyId);
                return Ok(team);
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ForbiddenException ex) { return StatusCode(403, new { message = ex.Message }); }
        }

        [HttpGet]
        public async Task<IActionResult> GetTeamsForCompany()
        {
            var teams = await _service.GetTeamsForCompanyAsync(CompanyId);
            return Ok(teams);
        }

        [HttpGet("for-AI-model")]
        public async Task<IActionResult> GetTeamsForAI()
        {
            var teams = await _service.GetTeamsForAiAsync(CompanyId);
            return Ok(teams);
        }


        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var created = await _service.CreateTeamAsync(CompanyId, dto);
                return CreatedAtAction(nameof(GetTeam), new { teamId = created.Id }, created);
            }
            catch (ConflictException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{teamId}")]
        public async Task<IActionResult> UpdateTeam(string teamId, [FromBody] UpdateTeamDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            try
            {
                var updated = await _service.UpdateTeamAsync(teamId, CompanyId, dto);
                return Ok(updated);
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ConflictException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpDelete("{teamId}")]
        public async Task<IActionResult> DeleteTeam(string teamId)
        {
            try
            {
                await _service.DeleteTeamAsync(teamId, CompanyId);
                return NoContent();
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost("{teamId}/employees")]
        public async Task<IActionResult> AddEmployee(string teamId, [FromBody] AddEmployeeDto emp)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            try
            {
                var added = await _service.AddEmployeeAsync(teamId, CompanyId, emp.EmployeeId);
                if (!added) return BadRequest(new { message = "Employee already in team" });
                
                return Ok(new { message = "Employee added" });
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }


        [HttpDelete("{teamId}/employees/{employeeId}")]
        public async Task<IActionResult> RemoveEmployee(string teamId, string employeeId)
        {
            try
            {
                var removed = await _service.RemoveEmployeeAsync(teamId, CompanyId, employeeId);

                if (!removed) return BadRequest(new { message = "Employee not found in team" });

                return Ok(new { message = "Employee removed" });
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // Helper to get company id from claims (single source of truth)
        private string GetCompanyIdFromClaims()
        {
            var claim = User.FindFirst("companyId") ?? User.FindFirst("tenant") ?? User.FindFirst(ClaimTypes.GroupSid);

            if (claim == null) 
                throw new ForbiddenException("Company claim not found");

            return claim.Value;
        }
    }
}
