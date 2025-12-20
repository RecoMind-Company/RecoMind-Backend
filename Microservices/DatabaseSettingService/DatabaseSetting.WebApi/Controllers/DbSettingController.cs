using DatabaseSetting.Core.DTOs;
using DatabaseSetting.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseSetting.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbSettingController : ControllerBase
    {
        private readonly IDbSettingService _service;
        public DbSettingController(IDbSettingService service)
        {
            _service = service;
        }


        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetByCompanyId(string companyId)
        {
            var result = await _service.GetByCompanyIdForAiAsync(companyId);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpGet("for-ai")]
        [Authorize(Policy = "Ai")]
        public async Task<IActionResult> GetForAiAsync()
        {
            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.GetByCompanyIdForAiAsync(companyId);
            if (result == null)  return NotFound();

            return Ok(result);
        }


        [HttpGet]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> GetForCompanyAsync()
        {
            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.GetByCompanyIdAsync(companyId);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> GetById(string id)
        {
            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.GetByIdAsync(id, companyId);
            if (result == null) return NotFound();

            return Ok(result);
        }


        [HttpPost("create")]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> Create([FromBody] CreateDbSettingDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.CreateAsync(request, companyId);
            if (result == null) return BadRequest("Failed to create DbSetting.");

            return Ok(result);
        }

        [HttpPut("update/{id}")]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateDbSettingDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.UpdateAsync(id, companyId, request);
            if (result == null) return NotFound("Invalid data");

            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> Delete(string id)
        {
            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId claim is missing.");

            var success = await _service.DeleteAsync(id, companyId);
            if (!success) return NotFound();

            return NoContent();
        }
    }
}