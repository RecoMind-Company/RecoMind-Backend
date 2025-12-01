using Core.Consts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingCycleController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IEnumerable<string> GetAllBillingCycles()
        {
            return Enum.GetNames(typeof(BillingCycle));
        }
    }
}
