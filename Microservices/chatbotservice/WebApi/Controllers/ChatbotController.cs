using Core.DTOs;
using Core.Services.Interface;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using RecoMindAuthenticationAPI.Grpc.Authentication;
using static RecoMindAuthenticationAPI.Grpc.Authentication.AuthenticationService;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatBotService _chatBotService;
        private readonly RecoMindAuthenticationAPI.Grpc.Authentication.AuthenticationService.AuthenticationServiceClient _authenticationServiceClient;
        public ChatbotController(IChatBotService chatBotService, AuthenticationServiceClient authenticationServiceClient)
        {
            _chatBotService = chatBotService;
            _authenticationServiceClient = authenticationServiceClient;
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuery(CreateChatRequestDto createChatRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                if (!string.IsNullOrEmpty(createChatRequestDto.UserID))
                {
                    var user = _authenticationServiceClient.GetUserById(new GetUserByIdMessage { UserId = createChatRequestDto.UserID });

                    if (user == null || !(user.Role.ToLower().Equals(createChatRequestDto.UserRole)))
                        throw new KeyNotFoundException($" Invalid UserId Or Role");
                }

                var result = await _chatBotService.HandelQuery(createChatRequestDto);
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
