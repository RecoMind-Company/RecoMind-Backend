using Core.Dtos;
using Core.ServicesAbstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}
