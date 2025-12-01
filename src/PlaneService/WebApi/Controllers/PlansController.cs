using Core.DTOs;
using Core.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _planService;
        public PlansController(IPlanService planService)
        {
            _planService = planService;
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
            var createdPlan = await _planService.CreatePlan(planDto);
            return CreatedAtAction(nameof(GetPlan), new { planId = createdPlan.Id }, createdPlan);
        }

        [HttpPut]       
        public async Task<IActionResult> UpdatePlan([FromQuery] string PlanId , [FromBody] CreatePlanDto planDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updatedPlan = await _planService.UpdatePlan(PlanId , planDto);
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

