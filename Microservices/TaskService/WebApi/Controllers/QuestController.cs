using Core.Dtos;
using Core.Result;
using Core.ServicesAbstractions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class QuestController(IQuestService questService,
                             IUserQuestsService userQuestsService,
                             IValidator<QuestDto> questDtoValidator,
                             IValidator<UpdateQuestDto> updateQuestDtoValidator,
                             IValidator<QuestByStatusDto> questByStatusDtoValidator) : BaseApiController
{
    [HttpPost("add-task")]
    [ProducesResponseType(typeof(QuestToReturnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuestToReturnDto>> CreateTask([FromBody] QuestDto questDto)
    {
        var validationResult = await ExecuteValidation(questDtoValidator, questDto);
        if (!validationResult.IsSuccess)
            return BadRequest(validationResult.ErrorList);
        var result = await questService.CreateQuestAsync(questDto);
        return result.Map<ActionResult<QuestToReturnDto>>(
            onSuccess: quest => Ok(quest),
            onFailure: err => HandleFailure(err));
    }

    [HttpGet("{planId}/tasks")]
    [ProducesResponseType(typeof(IEnumerable<QuestToReturnDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<QuestToReturnDto>>> GetAllTasksAsync([FromRoute] string planId, [FromQuery] string? moduleId)
    {
        var result = await questService.GetAllQuestsAsync(planId, moduleId);
        return result.Map<ActionResult<IEnumerable<QuestToReturnDto>>>(
            onSuccess: quests => Ok(quests),
            onFailure: err => HandleFailure(err));
    }

    [HttpGet("{questId}")]
    [ProducesResponseType(typeof(QuestToReturnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestToReturnDto>> GetTaskByIdAsync(string questId)
    {
        var result = await questService.GetQuestByIdAsync(questId);
        return result.Map<ActionResult<QuestToReturnDto>>(
            onSuccess: quest => Ok(quest),
            onFailure: err => HandleFailure(err));
    }

    [HttpGet("{planId}/by-status")]
    [ProducesResponseType(typeof(IEnumerable<QuestToReturnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<QuestToReturnDto>>> GetAllTasksByStatusAsync(string planId, [FromQuery] QuestByStatusDto questByStatusDto)
    {
        var validationResult = await ExecuteValidation(questByStatusDtoValidator, questByStatusDto);
        if (!validationResult.IsSuccess)
            return BadRequest(validationResult.ErrorList);
        var result = await questService.GetAllQuestsByStatusAsync(questByStatusDto, planId);
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
    [HttpDelete("delete/{questId}")]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteTaskAsync(string questId)
    {
        var result = await questService.DeleteQuestAsync(questId);
        return result.Map(
            onSuccess: _ => NoContent(),
            onFailure: err => HandleFailure(err));
    }
    [HttpPost("personal")]
    public async Task<ActionResult<PersonalQuestToReturnDto>> CreateTaskAndAssignUsers([FromBody] FullQuestDto fullQuestDto)
    {
        var validationResult = await ExecuteValidation(questDtoValidator, fullQuestDto.questDto);
        if (!validationResult.IsSuccess)
            return BadRequest(validationResult.ErrorList);
        var createdQuest = await questService.CreatePersonalQuestAsync(fullQuestDto.questDto);
        if (!createdQuest.IsSuccess)
            return HandleFailure(createdQuest.ErrorList);
        var finalResult = await userQuestsService.AssignUsersToQuestAsync(fullQuestDto.UserIds, createdQuest.Value!.QuestId);

        return finalResult.Map<ActionResult<PersonalQuestToReturnDto>>(
            onSuccess: quest => Ok(quest),
            onFailure: err => HandleFailure(err));
    }

}