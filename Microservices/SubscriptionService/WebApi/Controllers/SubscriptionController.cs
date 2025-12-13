using Core.Consts;
using Core.DTOs;
using Core.DTOs.SubscriptionTypeDto;
using Core.Service;
using Core.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "admin")]
    public class SubscriptionController : ControllerBase
    {
        private ISubscriptionCompanyService _subscriptionService;
        private readonly ISubscriptionTypeService _subscriptionTypeService;

        public SubscriptionController(ISubscriptionCompanyService subscriptionService , ISubscriptionTypeService subscriptionTypeService)
        {
            _subscriptionService = subscriptionService;
            _subscriptionTypeService = subscriptionTypeService;
        }

        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(IEnumerable<GetSubscriptionCompanyDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var subscriptions = await _subscriptionService.GetAllSubscriptions();
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }        

        [HttpGet("GetById/{id}")]
        [ProducesResponseType(typeof(GetSubscriptionCompanyDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var subscription = await _subscriptionService.GetSubscriptionById(id);
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllBillingCycles")]
        public IEnumerable<string> GetAllBillingCycles()
        {
            return Enum.GetNames(typeof(BillingCycle));
        }

        [HttpGet("AllSubscriptionType")]
        public async Task<IActionResult> GetAllSubscriptionType()
        {
            try
            {
                var result = await _subscriptionTypeService.GetAllSubscriptionPlan();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Create")]
        [ProducesResponseType(typeof(GetSubscriptionCompanyDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionCompanyDto subscriptionDto)
        {
            try
            {
                var createdSubscription = await _subscriptionService.CreateSubscription(subscriptionDto);
                return CreatedAtAction(nameof(GetById), new { id = createdSubscription.Id }, createdSubscription);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateSubscriptionType")]
        public async Task<IActionResult> CreateSubscriptionType(CreateDto dto)
        {
            try
            {
                var result = await _subscriptionTypeService.AddSubscriptionPlan(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Update/{id}")]
        [ProducesResponseType(typeof(UpdateSubscriptionCompanyDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(string id, [FromBody] CreateSubscriptionCompanyDto subscriptionDto)
        {
            try
            {
                var updatedSubscription = await _subscriptionService.UpdateSubscription(id, subscriptionDto);
                return Ok(updatedSubscription);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateSubscriptionType/{OldPlanTypeName}")]
        public async Task<IActionResult> UpdateSubscriptionType(string OldPlanTypeName, CreateDto subscriptionTypeDto) 
        {
            try
            {
                var result = await _subscriptionTypeService.UpdateSubscriptionType(OldPlanTypeName, subscriptionTypeDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("Delete/{id}")]
        [ProducesResponseType(typeof(DeleteSubscriptionCompanyDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var deletedSubscription = await _subscriptionService.DeleteSubscription(id);
                return Ok(deletedSubscription);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("SubscriptionType/{PlanName}")]
        public async Task<IActionResult> DeleteSubscriptionType(string PlanName)
        {
            try
            {
                await _subscriptionTypeService.DleteSubscriptionType(PlanName);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
