using Core.Dtos.Task;
using Core.Result;
using Core.ServicesAbstraction;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize]
public class TaskCommentController(IQuestCommentService questCommentService,
                                IValidator<AddTaskCommentDto> addTaskCommentDtoValidator,
                                IValidator<UpdateTaskCommentDto> updateTaskCommentDtoValidator) : BaseApiController
{
    [HttpPost("{questId}/add-comment")]
    [ProducesResponseType(typeof(TaskCommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskCommentDto>> AddCommentAsync([FromRoute] string questId,
                                                                   [FromBody] AddTaskCommentDto addCommentDto)
    {
        addCommentDto.QuestId = questId;
        addCommentDto.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var validationResult = await ExecuteValidation(addCommentDto, addTaskCommentDtoValidator);
        if (!validationResult.IsSuccess)
            return BadRequest(validationResult.ErrorsList);

        var result = await questCommentService.AddCommentAsync(addCommentDto);
        return result.Map<ActionResult<TaskCommentDto>>(
            onSuccess: comment => Ok(comment),
            onFailure: err => HandleFailure(err));
    }

    [HttpGet("{questId}/all")]
    [ProducesResponseType(typeof(IEnumerable<TaskCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TaskCommentDto>>> GetCommentsByTaskIdAsync([FromRoute] string questId)
    {
        var result = await questCommentService.GetCommentsByTaskIdAsync(questId);
        return result.Map<ActionResult<IEnumerable<TaskCommentDto>>>(
            onSuccess: comments => Ok(comments),
            onFailure: err => HandleFailure(err));
    }

    [HttpPut("update-comment")]
    [ProducesResponseType(typeof(TaskCommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskCommentDto>> UpdateCommentAsync([FromBody] UpdateTaskCommentDto updateCommentDto)
    {
        updateCommentDto.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var validationResult = await ExecuteValidation(updateCommentDto, updateTaskCommentDtoValidator);
        if (!validationResult.IsSuccess)
            return BadRequest(validationResult.ErrorsList);

        var result = await questCommentService.UpdateCommentAsync(updateCommentDto);
        return result.Map<ActionResult<TaskCommentDto>>(
            onSuccess: comment => Ok(comment),
            onFailure: err => HandleFailure(err));
    }

    [HttpDelete("{commentId}/remove")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteCommentAsync([FromRoute] string commentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await questCommentService.DeleteCommentAsync(commentId, userId);
        return result.Map<ActionResult<bool>>(
            onSuccess: _ => Ok(true),
            onFailure: err => HandleFailure(err));
    }
}




