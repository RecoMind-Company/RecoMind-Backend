using Core.Dtos;
using Core.Result;
using Core.ServicesAbstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;
[ApiController]
[Route("api")]
[Authorize]
public class CommentController(ICommentService commentService) : BaseApiController
{
    [HttpGet("plans/{planId}/comments/get-all")]
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await commentService.DeleteCommentAsync(commentId, userId!);
        return result.Map<ActionResult>(
                onSuccess: _ => NoContent(),
                onFailure: errors => HandleFailure(errors));
    }
}
