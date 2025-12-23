using Core.Const;
using Core.DTOs.AiService;
using Core.DTOs.Chatbot;
using Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatBotService _chatBotService;

        public ChatbotController(IChatBotService chatBotService)
        {
            _chatBotService = chatBotService;
        }

        [HttpPost("CreateQuery")]
        public async Task<IActionResult> CreateQuery(QuestionDto question)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var dto = new CreateChatRequestDto
                {
                    user_question = question.Question,
                    UserID = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                    UserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
                };
                var result = await _chatBotService.HandelRequestBeforeBassingItToAiService(dto);
                result.user_question = question.Question;
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
        public async Task<IActionResult> GetResult([FromBody] GetMethodDto dto)
        {
            try
            {
                var result = await _chatBotService.GetResponseFromAiService(dto);

                if (result.Status == Status.SUCCESS) // Save to DB
                {
                    var model = new SaveDto
                    {
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                        UserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
                        UserQuestion = dto.user_question,
                        TaskId = dto.TaskId,
                        Result = new ResponseMessage
                        {
                            Answer = result.Result.Answer,
                            Sql_Query = result.Result.Sql_Query
                        }
                    };

                    await _chatBotService.SaveToDatabase(model);
                }
                else if (result.Status == Status.FAILURE)
                {
                    return BadRequest("Failed to get response from AI service.");
                }

                var res = new UserResponseDto
                {
                    Status = result.Status,
                    Response = result.Result.Answer,
                };

                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetHistory")]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var result = await _chatBotService.GetHistory(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteHistory")]
        public async Task<IActionResult> DeleteHistory()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                await _chatBotService.DeleteHistory(userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
