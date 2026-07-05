using Azure.Core;
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
        private readonly ITeamService _service;
        public TeamController(ITeamService service) => _service = service;

        private string _companyId => User.FindFirstValue("CompanyId") ?? string.Empty;
        private string _userId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;



        [HttpGet("{teamId}/company/{company_id}")]
        public async Task<IActionResult> GetTeamJobTitlesAsync(string teamId, string company_id)
        {
            var jobTitles = await _service.GetTeamMemberJobTitlesAsync(teamId, company_id);
            if (jobTitles == null || jobTitles.Count == 0)
                return NotFound("Team not found or has no employees.");

            return Ok(jobTitles);
        }

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
            var teams = await _service.GetByCompanyIdAsync(_companyId);
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

        [HttpGet("by-leader")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> GetByLeaderId()
        {
            var result = await _service.GetTeamByEmployeeIdAsync(_userId);

            if (!result.IsSuccess)
                result = await _service.GetTeamByTeamLeadIdAsync(_userId);

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
            if (string.IsNullOrWhiteSpace(_companyId)) 
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.CreateTeamAsync(_companyId, dto);

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
            if (string.IsNullOrWhiteSpace(_companyId)) return BadRequest("CompanyId claim is missing.");

            var result = await _service.UpdateTeamAsync(teamId, _companyId, dto);

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
            if (string.IsNullOrWhiteSpace(_companyId)) return BadRequest("CompanyId claim is missing.");

            var result = await _service.DeleteTeamAsync(teamId, _companyId);

            return result.Map(
                success => NoContent(),
                error => (IActionResult)NotFound()
            );
        }



        [HttpGet("team-employees")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> GetTeamEmployees()
        {

            var employees = await _service.GetTeamEmployees(_userId);
            if (employees == null || employees.Count == 0)
                return NotFound("No employees found for this team.");

            return Ok(employees);
        }   


        [HttpPost("{teamId}/employees")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> AddEmployee(string teamId, EmployeeDto emp)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (string.IsNullOrWhiteSpace(_companyId)) return BadRequest("CompanyId claim is missing.");

            var result = await _service.AddEmployeeAsync(teamId, _companyId, emp.EmployeeId);

            return result.Map<IActionResult>(
                success => Ok(),
                error => BadRequest(new { message = "Cannot add employee to team." })
            );
        }

        [HttpDelete("{teamId}/employees/{employeeId}")]
        [Authorize(Policy = "AllEmployees")]
        public async Task<IActionResult> RemoveEmployee(string teamId, string employeeId)
        {
            if (string.IsNullOrWhiteSpace(_companyId)) return BadRequest("CompanyId claim is missing.");

            var result = await _service.RemoveEmployeeAsync(teamId, _companyId, employeeId);

            return result.Map<IActionResult>(
                success => NoContent(),
                error => BadRequest(new { message = "Cannot remove employee from team." })
            );
        }
    }
}
