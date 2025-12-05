using Core.DTOs.AiService;
using Core.DTOs.Chatbot;
using Core.Services.Interface;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using RecoMindAuthenticationAPI.Grpc.Authentication;
using WebApi.Grpc.ConnectedService;
using static RecoMindAuthenticationAPI.Grpc.Authentication.AuthenticationService;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatBotService _chatBotService;
        private readonly AuthService _authService;
        private readonly TeamService _teamService;
        public ChatbotController(IChatBotService chatBotService, AuthService authService , TeamService teamService)
        {
            _chatBotService = chatBotService;
            _authService = authService;
            _teamService = teamService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuery(CreateChatRequestDto createChatRequestDto)
        {
            if (!ModelState.IsValid)            
                return BadRequest(ModelState);
            
            if (!(await _authService.CheckValidUser(createChatRequestDto)&&!(await _teamService.CheckValidTeam(createChatRequestDto.UserID))))            
                return BadRequest("Request body Has Invalid Data ");
                     
            try
            { 
                var team = await _teamService.GetTeamByUserId(createChatRequestDto.UserID); 
                
                var Dto = new AiRequestDto
                {
                    compnay_id = team.CompanyId,
                    team_name = team.TeamName,
                    query = createChatRequestDto.Query
                };
                var result = await _chatBotService.HandelQuery(Dto);
                return Ok(result);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(string UserId)
        {
            var result = await _chatBotService.GetHistory(UserId);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteHistory(string UserId)
        {
            await _chatBotService.DeleteHistory(UserId);
            return NoContent();
        }
    }
}
