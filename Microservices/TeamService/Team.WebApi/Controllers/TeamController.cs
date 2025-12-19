using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using System.Security.Claims;
using Team.Core.DTOs;
using Team.Core.Interfaces;
using Team.Core.Services;

namespace Team.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private string _companyId => GetCompanyIdFromClaims();
        private readonly ITeamService _service;
        public TeamController(ITeamService service)
        {
            _service = service;
        }


        [HttpGet("for-ai")]
        [Authorize(Policy = "Ai")]
        public async Task<IActionResult> GetTeamsForAI()
        {
            var teams = await _service.GetForAiAsync(_companyId);
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

        [HttpGet]
        [Authorize(Policy = "Management")]
        public async Task<IActionResult> GetTeamsForCompany()
        {
            var teams = await _service.GetByCompanyIdAsync(_companyId);
            return Ok(teams);
        }


        [HttpPost]
        //[Authorize(Policy = "Management")]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var team = await _service.CreateTeamAsync(_companyId, dto);
                return Ok(team);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{teamId}")]
        [Authorize(Policy = "Management")]
        public async Task<IActionResult> UpdateTeam(string teamId, [FromBody] UpdateTeamDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                var team = await _service.UpdateTeamAsync(teamId, _companyId, dto);
                return Ok(team);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{teamId}")]
        [Authorize(Policy = "Management")]
        public async Task<IActionResult> DeleteTeam(string teamId)
        {
            var success = await _service.DeleteTeamAsync(teamId, _companyId);
            if (!success) return NotFound();

            return NoContent();
        }

        [HttpPost("{teamId}")]
        [Authorize(Policy = "TeamLeadership")]
        public async Task<IActionResult> AddEmployee(string teamId, [FromBody] AddEmployeeDto emp)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var success = await _service.AddEmployeeAsync(teamId, _companyId, emp.EmployeeId);
            if (!success) return BadRequest(new { message = "Cannot add employee to team." });

            return Ok();
        }


        [HttpDelete("{teamId}/employees/{employeeId}")]
        [Authorize(Policy = "TeamLeadership")]
        public async Task<IActionResult> RemoveEmployee(string teamId, string employeeId)
        {
            var success = await _service.RemoveEmployeeAsync(teamId, _companyId, employeeId);
            if (!success) return BadRequest(new { message = "Cannot remove employee from team." });

            return NoContent();
        }


        // Helper to get company id from claims(single source of truth)
        private string GetCompanyIdFromClaims()
        {
            return "C4843CF9-8A71-451B-8052-FB229E9313E5";
            var claim = User.FindFirst("CompanyId") ?? User.FindFirst("companyId");

            if (claim == null)
                return string.Empty;

            return claim.Value;
        }
    }
}
