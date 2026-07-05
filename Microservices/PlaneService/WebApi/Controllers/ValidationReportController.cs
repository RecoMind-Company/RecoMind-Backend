using Core.DTOs;
using Core.DTOs.AI.ValidationReport;
using Core.DTOs.AI.ValidationReport.AIResult;
using Core.DTOs.ValidationReport;
using Core.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        [ProducesResponseType(typeof(AIValidationReportResponseDto), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(ValidationReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGeneratedValidationReport(string taskId)
        {
            var result = await validationReportService.GetValidationReport(taskId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }
            return Ok(result.Value);
        }

        [HttpPost("add")]
        [ProducesResponseType(typeof(UserValidationReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddValidationReport([FromBody] UserValidationReportAddDto reportAddDto)
        {
            var validStatuses = new List<int> { 0, 1, 2, 3 };
            if (validStatuses.Contains(reportAddDto.Status) == false)
            {
                return BadRequest("Invalid status value. Status must be either 0 (UnderReview) or 1 (Draft) or 2 (Rejected) or 3 (Accepted).");
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            reportAddDto.CreatedBy = userId;

            var result = await validationReportService.AddValidationReport(reportAddDto);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }
            return Ok(result.Value);
        }

        [HttpPost("send")]
        [ProducesResponseType(typeof(BaseToReturnDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendValidationReport([FromBody] SendValidationReportDto sendValidationDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            sendValidationDto.CreatedBy = userId;

            var result = await validationReportService.SendValidationReport(sendValidationDto);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }
            return Ok(result.Value);
        }

        [HttpPatch("update")]
        [ProducesResponseType(typeof(UserValidationReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateValidationReportStatus([FromBody] UserUpdateReportDto updateReportDto)
        {
            var validStatuses = new List<int> { 0, 1, 2, 3 };
            if (validStatuses.Contains(updateReportDto.Status) == false)
            {
                return BadRequest("Invalid status value. Status must be either 0 (UnderReview) or 1 (Draft) or 2 (Rejected) or 3 (Accepted).");
            }
            var result = await validationReportService.UpdateValidationReport(updateReportDto);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }
            return Ok(result.Value);
        }
        [HttpGet("get/{ReportId}")]
        [ProducesResponseType(typeof(UserValidationReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetValidationReportById(string ReportId)
        {
            var result = await validationReportService.GetValidationReportById(ReportId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }
            return Ok(result.Value);
        }

        [HttpGet("sent")]
        [ProducesResponseType(typeof(IEnumerable<UserValidationReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserSentToValidationReports([FromQuery] int limit)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await validationReportService.GetValidationReportBySendToId(userId!, limit);
            return Ok(result.Value);
        }
        [HttpGet("by-status")]
        [ProducesResponseType(typeof(IEnumerable<UserValidationReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetValidationReportByStatus([FromQuery] int limit, [FromQuery] int status)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await validationReportService.GetValidationReportByStatus(userId!, status, limit);
            return Ok(result.Value);
        }
        [HttpGet("created-by-status")]
        [ProducesResponseType(typeof(IEnumerable<UserValidationReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCreatedByValidationReportByStatus([FromQuery] int limit, [FromQuery] int status)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await validationReportService.GetCreatedByValidationReportByStatus(createdBy: userId!, status, limit);
            return Ok(result.Value);
        }
        [HttpGet("created-user")]
        [ProducesResponseType(typeof(IEnumerable<UserValidationReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetuserCreatedValidationReports([FromQuery] int limit)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await validationReportService.GetValidationReportByCreatedById(userId!, limit);
            return Ok(result.Value);
        }
    }
}
