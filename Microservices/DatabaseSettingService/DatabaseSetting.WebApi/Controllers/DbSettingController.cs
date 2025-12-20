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

        [HttpGet("for-ai")]
        [Authorize(Policy = "Ai")]
        public async Task<IActionResult> GetForAiAsync()
        {
            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if(string.IsNullOrWhiteSpace(_companyId)) 
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.GetByCompanyIdForAiAsync(_companyId);
            if (result == null)  return NotFound();

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> GetForCompanyAsync()
        {
            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_companyId))
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.GetByCompanyIdAsync(_companyId);
            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> GetById(string id)
        {
            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_companyId))
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.GetByIdAsync(id, _companyId);
            if (result == null) return NotFound();

            return Ok(result);
        }


        [HttpPost]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> Create([FromBody] CreateDbSettingDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_companyId))
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.CreateAsync(request, _companyId);
            if (result == null) return BadRequest("Failed to create DbSetting.");

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateDbSettingDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_companyId))
                return BadRequest("CompanyId claim is missing.");

            var result = await _service.UpdateAsync(id, _companyId, request);
            if (result == null) return NotFound("Invalid data");

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "ManagerRole")]
        public async Task<IActionResult> Delete(string id)
        {
            var _companyId = User.FindFirst("CompanyId").Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_companyId))
                return BadRequest("CompanyId claim is missing.");

            var success = await _service.DeleteAsync(id, _companyId);
            if (!success) return NotFound();

            return NoContent();
        }


    }
}