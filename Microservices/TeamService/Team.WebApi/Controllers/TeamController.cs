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
            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if (string.IsNullOrEmpty(_companyId))
                return BadRequest(new { message = "Invalid company context." });

            var teams = await _service.GetForAiAsync(_companyId);
            return Ok(teams);
        }

        [HttpGet]
        [Authorize(Policy = "Management")]
        public async Task<IActionResult> GetTeamsForCompany()
        {
            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if (string.IsNullOrEmpty(_companyId))
                return BadRequest(new { message = "Invalid company context." });

            var teams = await _service.GetByCompanyIdAsync(_companyId);
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
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if (string.IsNullOrEmpty(_companyId))
                return BadRequest(new { message = "Invalid company context." });

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
        public async Task<IActionResult> UpdateTeam(string teamId, UpdateTeamDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if (string.IsNullOrEmpty(_companyId))
                return BadRequest(new { message = "Invalid company context." });

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
            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if (string.IsNullOrEmpty(_companyId))
                return BadRequest(new { message = "Invalid company context." });

            var success = await _service.DeleteTeamAsync(teamId, _companyId);
            if (!success) return NotFound();

            return NoContent();
        }


        [HttpPost("{teamId}")]
        [Authorize(Policy = "TeamLeadership")]
        public async Task<IActionResult> AddEmployee(string teamId, AddEmployeeDto emp)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if (string.IsNullOrEmpty(_companyId))
                return BadRequest(new { message = "Invalid company context." });

            var success = await _service.AddEmployeeAsync(teamId, _companyId, emp.EmployeeId);
            if (!success) return BadRequest(new { message = "Cannot add employee to team." });

            return Ok();
        }

        [HttpDelete("{teamId}/employees/{employeeId}")]
        [Authorize(Policy = "TeamLeadership")]
        public async Task<IActionResult> RemoveEmployee(string teamId, string employeeId)
        {
            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if (string.IsNullOrEmpty(_companyId))
                return BadRequest(new { message = "Invalid company context." });

            var success = await _service.RemoveEmployeeAsync(teamId, _companyId, employeeId);
            if (!success) return BadRequest(new { message = "Cannot remove employee from team." });

            return NoContent();
        }
    }
}
