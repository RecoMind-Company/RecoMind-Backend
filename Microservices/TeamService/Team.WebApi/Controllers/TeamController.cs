using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using System.Security.Claims;
using Team.Core.DTOs;
using Team.Core.Interfaces;

namespace Team.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _service;
        public TeamController(ITeamService service)
        {
            _service = service;
        }


        [HttpGet("for-ai")]
        [Authorize(Policy = "Ai")]
        public async Task<IActionResult> GetTeamsForAI()
        {
            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var teams = await _service.GetForAiAsync(companyId);
            return Ok(teams);
        }

        [HttpGet]
        [Authorize(Policy = "Management")]
        public async Task<IActionResult> GetTeamsForCompany()
        {
            var companyId = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var teams = await _service.GetByCompanyIdAsync(companyId);
            return Ok(teams);
        }

        [HttpGet("{teamId}")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> GetById(string teamId)
        {
            var team = await _service.GetByIdAsync(teamId);
            if (team == null) return NotFound();

            return Ok(team);
        }


        [HttpPost]
        [Authorize(Policy = "Management")]
        public async Task<IActionResult> CreateTeam(CreateTeamDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var companyId = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            try
            {
                var team = await _service.CreateTeamAsync(companyId, dto);
                return Ok(team);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{teamId}")]
        [Authorize(Policy = "Management")]
        public async Task<IActionResult> UpdateTeam(string teamId, UpdateTeamDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var companyId = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            try
            {
                var team = await _service.UpdateTeamAsync(teamId, companyId, dto);
                return Ok(team);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("{teamId}")]
        [Authorize(Policy = "Management")]
        public async Task<IActionResult> DeleteTeam(string teamId)
        {
            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var success = await _service.DeleteTeamAsync(teamId, companyId);
            if (!success) return NotFound();

            return NoContent();
        }


        [HttpPost("{teamId}")]
        [Authorize(Policy = "TeamLeadership")]
        public async Task<IActionResult> AddEmployee(string teamId, AddEmployeeDto emp)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var success = await _service.AddEmployeeAsync(teamId, companyId, emp.EmployeeId);
            if (!success) return BadRequest(new { message = "Cannot add employee to team." });

            return Ok();
        }

        [HttpDelete("{teamId}/employees/{employeeId}")]
        [Authorize(Policy = "TeamLeadership")]
        public async Task<IActionResult> RemoveEmployee(string teamId, string employeeId)
        {
            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var success = await _service.RemoveEmployeeAsync(teamId, companyId, employeeId);
            if (!success) return BadRequest(new { message = "Cannot remove employee from team." });

            return NoContent();
        }
    }
}
