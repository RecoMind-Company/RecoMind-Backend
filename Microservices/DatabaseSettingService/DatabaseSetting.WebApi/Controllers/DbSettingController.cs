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
        private string _companyId => GetCompanyIdFromClaims();

        private readonly IDbSettingService _service;
        public DbSettingController(IDbSettingService service)
        {
            _service = service;
        }

        [HttpGet("for-ai")]
        public async Task<IActionResult> GetForAiAsync()
        {
            var result = await _service.GetByCompanyIdForAiAsync(_companyId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        [Authorize("ManagerRole")]
        public async Task<IActionResult> GetForCompanyAsync()
        {
            var result = await _service.GetByCompanyIdAsync(_companyId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize("ManagerRole")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id, _companyId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }


        [HttpPost]
        [Authorize("ManagerRole")]
        public async Task<IActionResult> Create([FromBody] CreateDbSettingDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateAsync(request, _companyId);
            if (result == null)
                return BadRequest("Failed to create DbSetting.");

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize("ManagerRole")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateDbSettingDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateAsync(id, _companyId, request);

            if (result == null)
                return NotFound("Invalid data");

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize("ManagerRole")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteAsync(id, _companyId);
            if (!success) return NotFound();
            return NoContent();
        }


        // Helper to get company id from claims(single source of truth)
        private string GetCompanyIdFromClaims()
        {
            //return "fb140d33-7e96-474d-a06d-ab3a6c65d1a9";  // static companyId for testing

            var claim = User.FindFirst("CompanyId") ?? User.FindFirst("companyId");

            if (claim == null)
                return string.Empty;

            return claim.Value;
        }
    }
}