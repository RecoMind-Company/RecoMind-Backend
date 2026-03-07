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
                             IValidator<QuestByStatusDto> questByStatusDtoValidator) : BaseApiController
{
    [HttpPost("{planId}/add-task")]
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

}