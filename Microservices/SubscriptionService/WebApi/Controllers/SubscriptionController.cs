using Core.Consts;
using Core.DTOs;
using Core.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private ISubscriptionService _subscriptionService;
        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(IEnumerable<GetSubscriptionDto>), StatusCodes.Status200OK)]
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

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetSubscriptionDto), StatusCodes.Status200OK)]
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

        [HttpPost]
        [ProducesResponseType(typeof(GetSubscriptionDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionDto subscriptionDto)
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

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UpdateSubscriptionDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(string id, [FromBody] CreateSubscriptionDto subscriptionDto)
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

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(DeleteSubscriptionDto), StatusCodes.Status200OK)]
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
        public IEnumerable<string> GetAllPlans()
        {
            return Enum.GetNames(typeof(PlanType));
        }
        public IEnumerable<string> GetAllBillingCycles()
        {
            return Enum.GetNames(typeof(BillingCycle));
        }
    }
}
