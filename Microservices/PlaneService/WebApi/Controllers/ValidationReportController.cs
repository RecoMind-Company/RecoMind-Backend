using Core.DTOs.ValidationReport;
using Core.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ValidationReportController(IValidationReportService validationReportService) : ControllerBase
    {
        [HttpPost("generate")]
        public async Task<IActionResult> RequestValidationReport([FromBody] UserValidationReportRequestDto requestDto)
        {
            var companyId = User.FindFirst("CompanyId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId Not Found!");

            requestDto.CompanyId = companyId;
            requestDto.UserId = userId;
            var result = await validationReportService.RequestValidationReport(requestDto);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }
            return Ok(result.Value);
        }

        [HttpGet("generated/{taskId}")]
        public async Task<IActionResult> GetGeneratedValidationReport(string taskId)
        {
            var result = await validationReportService.GetValidationReport(taskId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }
            return Ok(result.Value);
        }
        //[HttpPost("add")]
        // if user want to save it as UnderReview
        // if user want to save it as Draft
        // if user want to send it as Accepted

        // [HttpPatch("update")]
        // update validation report to rejected or Accepted
    }
}
