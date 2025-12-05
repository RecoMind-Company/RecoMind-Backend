using DatabaseSetting.Core.DTOs;
using DatabaseSetting.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace DatabaseSetting.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbSettingController : ControllerBase
    {
        private readonly IDbSettingService _service;
        private readonly ILogger<DbSettingController> _logger;
        private string companyId => GetCompanyIdFromClaims();

        public DbSettingController(IDbSettingService service, ILogger<DbSettingController> logger)
        {
            _service = service;
            _logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllByCompanyIdAsync(companyId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id, companyId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("connection/{id}")]
        public async Task<IActionResult> GetConnectionStringById(string id)
        {
            var result = await _service.GetConnectionByIdAsync(id, companyId);

            if (result == null)
                return NotFound();

            return Ok(
                new
                {
                    Server = result.Server,
                    DatabaseName = result.DbName,
                    User = result.User,
                    Password = result.Password,
                    companyId = result.CompanyId
                });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDbSettingModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateAsync(request, companyId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id, companyId = result.CompanyId },
                result
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateDbSettingModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateAsync(id, companyId, request);

            if (result == null)
                return NotFound($"DbSetting with Id {id} not found for this company.");

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteAsync(id, companyId);
            if (!success) return NotFound();
            return NoContent();
        }

        // Helper to get company id from claims (single source of truth)
        private string GetCompanyIdFromClaims()
        {
            var claim = User.FindFirst("companyId") ?? User.FindFirst("tenant") ?? User.FindFirst(ClaimTypes.GroupSid);

            if (claim == null)
                throw new Exception("Company claim not found");

            return claim.Value;
        }
    }
}