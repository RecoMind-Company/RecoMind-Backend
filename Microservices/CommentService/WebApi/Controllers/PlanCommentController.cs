using Core.Dtos.Plan;
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
public class PlanCommentController(IPlanCommentService commentService,
                                   IValidator<AddPlanCommentDto> addCommentDtoValidator,
                                   IValidator<UpdatePlanCommentDto> updateCommentDtoValidator) : BaseApiController
{
    [HttpPost("{planId}/add-comment")]
    [ProducesResponseType(typeof(PlanCommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlanCommentDto>> AddCommentAsync([FromRoute] string planId, [FromBody] AddPlanCommentDto addCommentDto)
    {
        addCommentDto.PlanId = planId;
        addCommentDto.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var validationResult = await ExecuteValidation(addCommentDto, addCommentDtoValidator);
        if (!validationResult.IsSuccess)
            return BadRequest(validationResult.ErrorsList);

        var result = await commentService.AddCommentAsync(addCommentDto);
        return result.Map<ActionResult<PlanCommentDto>>(
            onSuccess: comment => Ok(comment),
            onFailure: err => HandleFailure(err));
    }

    [HttpGet("{planId}/all")]
    [ProducesResponseType(typeof(IEnumerable<PlanCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PlanCommentDto>>> GetCommentsByPlanIdAsync([FromRoute] string planId)
    {
        var result = await commentService.GetCommentsByPlanIdAsync(planId);
        return result.Map<ActionResult<IEnumerable<PlanCommentDto>>>(
            onSuccess: comments => Ok(comments),
            onFailure: err => HandleFailure(err));
    }

    [HttpPut("update-comment")]
    [ProducesResponseType(typeof(PlanCommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanCommentDto>> UpdateCommentAsync([FromBody] UpdatePlanCommentDto updateCommentDto)
    {
        updateCommentDto.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var validationResult = await ExecuteValidation(updateCommentDto, updateCommentDtoValidator);
        if (!validationResult.IsSuccess)
            return BadRequest(validationResult.ErrorsList);

        var result = await commentService.UpdateCommentAsync(updateCommentDto);
        return result.Map<ActionResult<PlanCommentDto>>(
            onSuccess: comment => Ok(comment),
            onFailure: err => HandleFailure(err));
    }

    [HttpDelete("{commentId}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteCommentAsync([FromRoute] string commentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await commentService.DeleteCommentAsync(commentId, userId);
        return result.Map<ActionResult<bool>>(
            onSuccess: _ => Ok(true),
            onFailure: err => HandleFailure(err));
    }
}