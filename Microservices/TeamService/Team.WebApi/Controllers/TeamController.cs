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
        public TeamController(ITeamService service) => _service = service;

        private string companyId => User.FindFirstValue("CompanyId") ?? string.Empty;


        [HttpGet("company/{company_id}")]
        public async Task<IActionResult> GetTeamsForAI(string company_id)
        {
            var teams = await _service.GetForAiAsync(company_id);
            if (teams == null || teams.Count == 0)
                return NotFound("No teams found for the specified company.");

            return Ok(teams);
        }

        [HttpGet("get-all")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> GetTeamsForCompany()
        {
            var teams = await _service.GetByCompanyIdAsync(companyId);
            if (teams == null || teams.Count == 0)
                return NotFound("No teams found for the specified company.");

            return Ok(teams);
        }

        [HttpGet("{teamId}")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> GetById(string teamId)
        {
            var result = await _service.GetByIdAsync(teamId);

            return result.Map(
                team => Ok(team),
                error => (IActionResult)NotFound()
            );
        }

        [HttpPost("create")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> CreateTeam(CreateTeamDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (string.IsNullOrWhiteSpace(companyId)) 
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.CreateTeamAsync(companyId, dto);

            return result.Map(
                team => (IActionResult)Ok(team),
                error => BadRequest(new { message = error.Message })
            );
        }

        [HttpPut("update/{teamId}")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> UpdateTeam(string teamId, UpdateTeamDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("CompanyId claim is missing.");

            var result = await _service.UpdateTeamAsync(teamId, companyId, dto);

            return result.Map(
                team => Ok(team),
                error => error.Code == "Team.NotFound"
                    ? (IActionResult)NotFound()
                    : BadRequest(new { message = error.Message })
            );
        }


        [HttpDelete("delete/{teamId}")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> DeleteTeam(string teamId)
        {
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("CompanyId claim is missing.");

            var result = await _service.DeleteTeamAsync(teamId, companyId);

            return result.Map(
                success => NoContent(),
                error => (IActionResult)NotFound()
            );
        }

        [HttpPost("{teamId}/employees")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> AddEmployee(string teamId, AddEmployeeDto emp)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("CompanyId claim is missing.");

            var result = await _service.AddEmployeeAsync(teamId, companyId, emp.EmployeeId);

            return result.Map<IActionResult>(
                success => Ok(),
                error => BadRequest(new { message = "Cannot add employee to team." })
            );
        }

        [HttpDelete("{teamId}/employees/{employeeId}")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> RemoveEmployee(string teamId, string employeeId)
        {
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("CompanyId claim is missing.");

            var result = await _service.RemoveEmployeeAsync(teamId, companyId, employeeId);

            return result.Map<IActionResult>(
                success => NoContent(),
                error => BadRequest(new { message = "Cannot remove employee from team." })
            );
        }
    }
}
