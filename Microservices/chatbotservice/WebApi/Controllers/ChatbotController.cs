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
using static System.Net.WebRequestMethods;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatBotService _chatBotService;
        private readonly AuthService _authService;
        //private readonly TeamService _teamService;
        public ChatbotController(IChatBotService chatBotService, AuthService authService)// ,TeamService teamService)
        {
            _chatBotService = chatBotService;
            _authService = authService;
            //_teamService = teamService;
    
           
            
        }

        [HttpPost("CreateQuery")]
        public async Task<IActionResult> CreateQuery(CreateChatRequestDto createChatRequestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //if (!(await _authService.CheckValidUser(createChatRequestDto)))//&& !(await _teamService.CheckValidTeam(createChatRequestDto.UserID))))
            //    return BadRequest("Request body Has Invalid Data ");


            try
            {
                //var team = await _teamService.GetTeamByUserId(createChatRequestDto.UserID);

                var Dto = new AiRequestDto
                {
                    company_id = "fb140d33-7e96-474d-a06d-ab3a6c65d1a9", //team.CompanyId,
                    team_name = "Sales", //team.TeamName,
                    user_question = createChatRequestDto.user_question
                };
                var result = await _chatBotService.SendQuryToAiService(Dto);
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

        [HttpPost("ChatbotResponse")]
        public async Task<IActionResult> GetResult(GetMethodDto dto)
        {
            try 
            {
                var result = await _chatBotService.GetResponseFromAiService(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetHistory")]
        public async Task<IActionResult> GetHistory(string UserId)
        {
            var result = await _chatBotService.GetHistory(UserId);
            return Ok(result);
        }

        [HttpDelete("DeleteHistory")]
        public async Task<IActionResult> DeleteHistory(string UserId)
        {
            await _chatBotService.DeleteHistory(UserId);
            return NoContent();
        }
    }
}
