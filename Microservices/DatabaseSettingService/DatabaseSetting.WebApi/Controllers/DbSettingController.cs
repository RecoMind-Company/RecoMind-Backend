using DatabaseSetting.Core.DTOs;
using DatabaseSetting.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DatabaseSetting.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbSettingController : ControllerBase
    {
        private readonly IDbSettingService _service;
        private readonly ILogger<DbSettingController> _logger;

        public DbSettingController(IDbSettingService service, ILogger<DbSettingController> logger)
        {
            _service = service;
            _logger = logger;
        }


        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetAllByCompanyId(string companyId)
        {
            var result = await _service.GetAllByCompanyIdAsync(companyId);

            return Ok(result);
        }

        [HttpGet("{id}/company/{companyId}")]
        public async Task<IActionResult> GetById(string id, string companyId)
        {
            var result = await _service.GetByIdAsync(id, companyId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }


        [HttpGet("connection/{id}/company/{companyId}")]
        public async Task<IActionResult> GetConnectionById(string id, string companyId)
        {
            var result = await _service.GetConnectionByIdAsync(id, companyId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDbSettingModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id, companyId = result.CompanyId },
                result
            );
        }


        [HttpPut("{id}/company/{companyId}")]
        public async Task<IActionResult> Update(string id, string companyId, [FromBody] UpdateDbSettingModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateAsync(id, companyId, request);

            if (result == null)
                return NotFound($"DbSetting with Id {id} not found for this company.");

            return Ok(result);
        }


        [HttpDelete("{id}/company/{companyId}")]
        public async Task<IActionResult> Delete(string id, string companyId)
        {
            var success = await _service.DeleteAsync(id, companyId);
            if (!success)
                return NotFound();

            return NoContent();
        }


    }
}