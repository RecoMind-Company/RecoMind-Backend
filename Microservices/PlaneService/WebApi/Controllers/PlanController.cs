using Core.DTOs.PlanDtos;
using Core.DTOs.PlnaTypeDtos;
using Core.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace webApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]//(Roles = "admin")]

    public class PlanController : ControllerBase
    {
        public IPlanService _planService;
        public IPlanType _planTypeService;
        public IStatus _statusService;
        public PlanController(IPlanService PlanService, IPlanType planType, IStatus StatusService)
        {
            _planService = PlanService;
            _planTypeService = planType;
            _statusService = StatusService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllPlans()
        {
            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest(" CompanyId Not Found !");

            var plans = await _planService.GetAllPlans(companyId);

            return Ok(plans);
        }

        [HttpGet("GetId/{id}")]
        public async Task<IActionResult> GetPlanById(string id)
        {
            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest(" CompanyId Not Found !");

            var plan = await _planService.GetPlanById(id, companyId);

            if (plan.IsSuccess)
                return Ok(plan);

            return NotFound(plan.Error);
        }

        [HttpGet("GetByStatus/{status}")]
        public async Task<IActionResult> GetPlanByStatus([FromRoute] string status)
        {
            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest(" CompanyId Not Found !");

            var plans = await _planService.GetPlansByStatus(status, companyId);

            if (plans.Any(x => x.IsFailure))
                return NotFound(plans.Any(x => x.IsFailure));
            else
                return Ok(plans);
        }

        [HttpPost("CreatePlan")]
        public async Task<IActionResult> CreatePlan([FromBody] AddPlanDto createPlanDto)
        {
            var companyId = User.FindFirst("CompanyId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest(" CompanyId Not Found !");

            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(" User Not Found !");

            var createdPlan = await _planService.CreatePlan(createPlanDto, companyId, userId);

            if (createdPlan.IsSuccess)
                return Ok(createdPlan);
            else
                return BadRequest(createdPlan);
        }

        [HttpPut("UpdatePlan")]
        public async Task<IActionResult> UpdatePlan([FromBody] UpdatePlanDto updatePlanDto)
        {
            var companyId = User.FindFirst("CompanyId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest(" CompanyId Not Found !");

            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(" User Not Found !");

            var updatedPlan = await _planService.UpdatePlan(companyId, userId, updatePlanDto);

            if (updatedPlan.IsSuccess)
                return Ok(updatedPlan);
            else
                return BadRequest(updatedPlan.Error);
        }

        [HttpDelete("Remove")]
        public async Task<IActionResult> DeletePlan(string planId)
        {
            var companyId = User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest(" CompanyId Not Found !");

            var result = await _planService.DeletePlan(planId, companyId);
            if (result)
                return Ok($"Plan with ID {planId} deleted successfully.");
            else
                return NotFound($"Plan with ID {planId} not found.");
        }

        [HttpPost("PlanType/Add")]
        public async Task<IActionResult> AddPlanType(AddPlantypeDto Dto)
        {
            var result = await _planTypeService.AddPlanType(Dto);
            if (result)
            {
                return Ok($"Plan type {Dto.Name} added successfully.");
            }
            else
            {
                return BadRequest("Failed to add plan type.");
            }
        }

        [HttpGet("PlanType/GetAll")]
        public async Task<IActionResult> GetAllPlanTypes()
        {
            var result = await _planTypeService.GetAllPlanTypes();

            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound("No plan types found.");
            }
        }

        [HttpDelete("PlanType/Remove/{PlanTypeName}")]
        public async Task<IActionResult> DeletePlanType([FromRoute] string PlanTypeName)
        {
            var item = await _planTypeService.DeletePlanType(PlanTypeName);
            if (item)
                return Ok($"Plan Type {PlanTypeName} Deleted Successfuly ");

            return NotFound($"Plan Type With Name {PlanTypeName} Not Found");
        }

        [HttpGet("Status/GetAll")]
        public async Task<IActionResult> GetAllStatuses()
        {
            var result = await _statusService.GetAllStatuses();
            return Ok(result);
        }

        [HttpPost("Status/Add/{Name}")]
        public async Task<IActionResult> AddStatus([FromRoute] string Name)
        {
            var Check = await _statusService.AddStatus(Name);
            if (Check)
                return Ok(" Added Successfuly ");
            return BadRequest(" Try Again ");
        }

        [HttpDelete("Status/Remove/{Name}")]
        public async Task<IActionResult> DeleteStatuse([FromRoute] string Name)
        {
            var Check = await _statusService.DeleteStatus(Name);
            if (Check)
                return Ok(" Deleted Successfuly ");
            return BadRequest(" Status Not Found ");
        }

        [HttpPost("custom-plan/generate")]
        public async Task<IActionResult> GenerateCustomPlan([FromBody] UserCustomPlanDto userCustomPlanDto)
        {
            //var companyId = User.FindFirst("CompanyId")?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //  FOR TEST 

            var companyId = "34293b50-0c58-4111-8fcd-b0127dd250ce";

            if (string.IsNullOrWhiteSpace(companyId))
                return BadRequest("CompanyId Not Found!");

            var response = await _planService.CreateCustomPlan(userCustomPlanDto, companyId, userId);
            if (response.IsSuccess)
                return Ok(response.Value);
            else
                return BadRequest(response.Error);
        }
    }
}
