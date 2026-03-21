using Core.Dtos;
using Core.Result;
using Core.ServicesAbstraction;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
[ApiController]
[Route("api")]
public class CommentController(ICommentService commentService,
                                IValidator<AddCommentDto> addCommentDtoValidator) : BaseApiController
{
    [HttpPost("plans/{planId}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommentDto>> CreateComment([FromRoute] string planId, [FromBody] AddCommentDto addCommentDto)
    {
        var validationResult = await ExecuteValidation(addCommentDto, addCommentDtoValidator);
        if (!validationResult.IsSuccess)
            return HandleFailure(validationResult.ErrorsList);

        //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //TODO: Placeholder for user ID, replace with actual user ID from claims
        var userId = "user!@#123";
        addCommentDto.UserId = userId;
        addCommentDto.PlanId = planId;

        var result = await commentService.AddCommentAsync(addCommentDto);
        return result.Map<ActionResult<CommentDto>>(
                onSuccess: comment => Ok(comment),
                onFailure: errors => HandleFailure(errors));
    }
    [HttpGet("plans/{planId}/comments")]
    [ProducesResponseType(typeof(IEnumerable<CommentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByPlanId([FromRoute] string planId)
    {
        var result = await commentService.GetCommentsByPlanIdAsync(planId);
        return result.Map<ActionResult<IEnumerable<CommentDto>>>(
                onSuccess: comments => Ok(comments),
                onFailure: _ => HandleFailure(_));
    }
    [HttpDelete("comments/{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteComment([FromRoute] string commentId)
    {
        //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //TODO: Placeholder for user ID, replace with actual user ID from claims
        var userId = "user!@#123";
        var result = await commentService.DeleteCommentAsync(commentId, userId);
        return result.Map<ActionResult>(
                onSuccess: _ => NoContent(),
                onFailure: errors => HandleFailure(errors));
    }
}
