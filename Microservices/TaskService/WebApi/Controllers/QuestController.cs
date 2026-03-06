using Core.Dtos;
using Core.Result;
using Core.ServicesAbstractions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/Tasks")]
public class QuestController(IQuestService questService,
                             IValidator<QuestDto> questDtoValidator) : BaseApiController
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
}