using Core.Dtos;
using Core.Result;
using Core.ServicesAbstractions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/Tasks")]
public class QuestController(IQuestService questService,
                             IValidator<QuestDto> questDtoValidator,
                             IValidator<UpdateQuestDto> updateQuestDtoValidator,
                             IValidator<AddUserToQuestDto> addUserToQuestValidator) : BaseApiController
{
    [HttpPost("{planId}/task")]
    [ProducesResponseType(typeof(QuestToReturnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuestToReturnDto>> CreateTask([FromBody] QuestDto questDto, string planId)
    {
        var validationResult = await ExecuteValidation(questDtoValidator, questDto);
        if (!validationResult.IsSuccess)
            return BadRequest(validationResult.ErrorList);
        var result = await questService.CreateQuestAsync(questDto, planId);
        return result.Map<ActionResult<QuestToReturnDto>>(
            onSuccess: quest => Ok(quest),
            onFailure: err => HandleFailure(err));
    }
    [HttpGet("{planId}/tasks")]
    [ProducesResponseType(typeof(IEnumerable<QuestToReturnDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<QuestToReturnDto>>> GetAllTasksAsync(string planId)
    {
        var result = await questService.GetAllQuestsAsync(planId);
        return result.Map<ActionResult<IEnumerable<QuestToReturnDto>>>(
            onSuccess: quests => Ok(quests),
            onFailure: err => HandleFailure(err));
    }
    [HttpPatch("update/{questId}")]
    [ProducesResponseType(typeof(QuestToReturnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestToReturnDto>> EditTaskAsync([FromBody] UpdateQuestDto updateQuestDto, string questId)
    {
        var validationResult = await ExecuteValidation(updateQuestDtoValidator, updateQuestDto);
        if (!validationResult.IsSuccess)
            return BadRequest(validationResult.ErrorList);
        var result = await questService.EditQuestAsync(updateQuestDto, questId);
        return result.Map<ActionResult<QuestToReturnDto>>(
            onSuccess: quest => Ok(quest),
            onFailure: err => HandleFailure(err));
    }
    [HttpPost("add-user-to-task")]
    [ProducesResponseType(typeof(QuestToReturnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestToReturnDto>> AddUserToTaskAsync(AddUserToQuestDto userToQuestDto)
    {
        var validationResult = await ExecuteValidation(addUserToQuestValidator, userToQuestDto);
        if (!validationResult.IsSuccess)
            return BadRequest(validationResult.ErrorList);
        var result = await questService.AddUserToQuestAsync(userToQuestDto);
        return result.Map<ActionResult<QuestToReturnDto>>(
            onSuccess: quest => Ok(quest),
            onFailure: err => HandleFailure(err));
    }
    [HttpGet("user-tasks")]
    public async Task<ActionResult<IEnumerable<QuestToReturnDto>>> GetUserAssignedTasksAsync()
    {
        // TODO: Refactor this to get the userId from the token, and add authorization to the endpoint.
        //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = "testUser1";
        var result = await questService.GetUserAssignedQuestsAsync(userId!);
        return result.Map<ActionResult<IEnumerable<QuestToReturnDto>>>(
            onSuccess: quests => Ok(quests),
            onFailure: err => HandleFailure(err));
    }
}