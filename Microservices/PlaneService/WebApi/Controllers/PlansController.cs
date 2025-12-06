using Core.DTOs;
using Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Team.Grpc;
using static Team.Grpc.TeamGrpcService;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _planService;
        private readonly TeamGrpcServiceClient _teamGrpcServiceClient;

        public PlansController(IPlanService planService, TeamGrpcServiceClient teamGrpcServiceClient)
        {
            _planService = planService;
            _teamGrpcServiceClient = teamGrpcServiceClient;
        }

        [HttpGet("{planId}")]
        public async Task<IActionResult> GetPlan(string planId)
        {
            var plan = await _planService.GetPlan(planId);
            return Ok(plan);
        }

        [HttpGet("team/{teamId}")]
        public async Task<IActionResult> GetAllPlansByTeamId(string teamId)
        {
            var plans = await _planService.GetAllPlansByTeamId(teamId);
            return Ok(plans);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody] CreatePlanDto planDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrEmpty(planDto.TeamId))
            {
                //GetTeamByIdRequest teamId = new GetTeamByIdRequest() { TeamId = planDto.TeamId };
                //var validTeamId = _teamGrpcServiceClient.GetTeamBasicInfo(teamId);
                //if (validTeamId == null)
                //{
                //    throw new KeyNotFoundException($"Team With Id {planDto.TeamId} Not Found !");
                //}
            }
            var createdPlan = await _planService.CreatePlan(planDto);
            return CreatedAtAction(nameof(GetPlan), new { planId = createdPlan.Id }, createdPlan);
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePlan([FromQuery] string PlanId, [FromBody] CreatePlanDto planDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!string.IsNullOrEmpty(planDto.TeamId))
            {
                GetTeamByIdRequest teamId = new GetTeamByIdRequest() { TeamId = planDto.TeamId };
                var validTeamId = _teamGrpcServiceClient.GetTeamBasicInfo(teamId);
                if (validTeamId == null)
                {
                    throw new KeyNotFoundException($"Team With Id {planDto.TeamId} Not Found !");
                }
            }
            var updatedPlan = await _planService.UpdatePlan(PlanId, planDto);
            return Ok(updatedPlan);
        }

        [HttpDelete("{planId}")]
        public async Task<IActionResult> DeletePlan(string planId)
        {
            var deleteResult = await _planService.DeletePlan(planId);
            return Ok(deleteResult);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPlans()
        {
            var plans = await _planService.GetAllPlans();
            return Ok(plans);
        }
    }
}

