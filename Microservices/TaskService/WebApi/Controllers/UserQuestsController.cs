using Core.Dtos;
using Core.Result;
using Core.ServicesAbstractions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("api/user-tasks")]
[Authorize]
public class UserQuestsController(IUserQuestsService userQuestsService,
                                  IValidator<AddUserToQuestDto> addUserToQuestValidator) : BaseApiController
{
    [HttpPost("add-user-to-task")]
    [ProducesResponseType(typeof(QuestToReturnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestToReturnDto>> AddUserToTaskAsync(AddUserToQuestDto userToQuestDto)
    {
        var validationResult = await ExecuteValidation(addUserToQuestValidator, userToQuestDto);
        if (!validationResult.IsSuccess)
            return BadRequest(validationResult.ErrorList);
        var result = await userQuestsService.AddUserToQuestAsync(userToQuestDto);
        return result.Map<ActionResult<QuestToReturnDto>>(
            onSuccess: quest => Ok(quest),
            onFailure: err => HandleFailure(err));
    }

    [HttpGet("user-tasks")]
    public async Task<ActionResult<IEnumerable<QuestToReturnDto>>> GetUserAssignedTasksAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await userQuestsService.GetUserAssignedQuestsAsync(userId!);
        return result.Map<ActionResult<IEnumerable<QuestToReturnDto>>>(
            onSuccess: quests => Ok(quests),
            onFailure: err => HandleFailure(err));
    }

    [HttpDelete("{questId}/users/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UnAssignUserFromTaskAsync(string questId, string userId)
    {
        var result = await userQuestsService.UnAssignUserFromQuestAsync(questId, userId);
        return result.Map(
            onSuccess: _ => NoContent(),
            onFailure: err => HandleFailure(err));
    }
}
