using Data_Mapping.Core.DTOs;
using Data_Mapping.Core.Interfaces;
using Data_Mapping.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Data_Mapping.WebApi.Controllers
{
    [Authorize(Policy = "Management")]
    [Route("api/[controller]")]
    [ApiController]
    public class MappingController : ControllerBase
    {
        private readonly IMappingService _service;
        private readonly MappingDbContext _context;


        public MappingController(IMappingService service, MappingDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet("available/{companyId}/{deptName}")]
        public async Task<IActionResult> GetAvailable(string companyId, string deptName)
        {
            try
            {
                var result = await _service.GetAvailableTablesAsync(companyId, deptName);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("review/{companyId}/{deptName}")]
        public async Task<IActionResult> GetForReview(string companyId, string deptName)
        {
            var result = await _service.GetTablesByDeptAsync(companyId, deptName);

            if (result == null || !result.Any())
            {
                return Ok(new { message = "No tables found for this department", data = result });
            }

            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToDept([FromBody] MappingRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.AddTablesToDeptAsync(
                request.CompanyId, request.DeptName, request.TableIds);

            if (!result) return BadRequest();

            return Ok(new { success = result });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> RemoveFromDept([FromBody] MappingRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.RemoveTablesFromDeptAsync(
                request.CompanyId, request.DeptName, request.TableIds);

            if (!result) return BadRequest();

            return Ok(new { success = result });
        }
    }
}
