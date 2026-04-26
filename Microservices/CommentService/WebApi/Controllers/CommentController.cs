using Core.Dtos;
using Core.Result;
using Core.ServicesAbstraction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;
[ApiController]
[Route("api/[Controller]")]
[Authorize]
public class CommentController(ICommentService commentService) : BaseApiController
{
    [HttpGet("plans/{planId}")]
    [ProducesResponseType(typeof(IEnumerable<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByPlanId([FromRoute] string planId)
    {
        var result = await commentService.GetCommentsByPlanIdAsync(planId);
        return result.Map<ActionResult<IEnumerable<CommentDto>>>(
                onSuccess: comments => Ok(comments),
                onFailure: _ => HandleFailure(_));
    }
}
