using Core.Consts;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanTypeController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IEnumerable<string> GetAllPlans()
        {
            return Enum.GetNames(typeof(PlanType));
        }
    }
}
