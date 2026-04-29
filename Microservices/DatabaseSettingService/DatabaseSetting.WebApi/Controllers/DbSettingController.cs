using DatabaseSetting.Core.DTOs;
using DatabaseSetting.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DatabaseSetting.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbSettingController : ControllerBase
    {
        private readonly IDbSettingService _service;
        public DbSettingController(IDbSettingService service) => _service = service;

        private string companyId => User.FindFirstValue("CompanyId") ?? string.Empty;


        [HttpGet("company/{company_Id}")]
        public async Task<IActionResult> GetByCompanyId(string company_Id)
        {
            var result = await _service.GetByCompanyIdForAiAsync(company_Id);

            return result.Map<IActionResult>(
                res => Ok(res),
                error => NotFound()
            );
        }

        [HttpGet]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> GetForCompanyAsync()
        {
            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.GetByCompanyIdAsync(companyId);

            return result.Map<IActionResult>(
                res => Ok(res),
                error => NotFound()
            );
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.GetByIdAsync(id, companyId);

            return result.Map<IActionResult>(
                res => Ok(res),
                error => NotFound()
            );
        }

        [HttpPost("create")]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> Create([FromBody] CreateDbSettingDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("CompanyId claim is missing.");

            var result = await _service.CreateAsync(request, companyId);

            return result.Map<IActionResult>(
                res => Ok(res),
                error => BadRequest(new { message = error.Message })
            );
        }

        [HttpPut("update/{id}")]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateDbSettingDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("CompanyId claim is missing.");

            var result = await _service.UpdateAsync(id, companyId, request);

            return result.Map<IActionResult>(
                res => Ok(res),
                error => NotFound(new { message = "Invalid data" })
            );
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(companyId)) return BadRequest("CompanyId claim is missing.");

            var result = await _service.DeleteAsync(id, companyId);

            return result.Map<IActionResult>(
                success => NoContent(),
                error => NotFound()
            );
        }
    }
}