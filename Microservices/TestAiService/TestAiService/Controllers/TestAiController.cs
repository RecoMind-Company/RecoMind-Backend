using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestAiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestAiController : ControllerBase
    {
        [HttpGet("connection-string")]
        public async Task<IActionResult> GetConnectionString()
        {
            return Ok(new
            {
                Server = StaticData.server,
                DatabaseName = StaticData.databaseName,
                User = StaticData.user,
                Password = StaticData.password,
                CompanyId = StaticData.companyId,
            });
        }

        [HttpGet("team-names")]
        public async Task<IActionResult> GetTeamNames(string companyID)
        {
            if (companyID != StaticData.companyId)
                return BadRequest("Invalid Company ID");

            return Ok(StaticData.tableNames);
        }

    }
}
