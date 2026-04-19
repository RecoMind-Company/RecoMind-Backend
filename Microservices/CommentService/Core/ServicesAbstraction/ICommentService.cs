using Core.Dtos;
using Core.Result;

namespace Core.ServicesAbstraction;

public interface ICommentService
{
    Task<Result<CommentDto>> AddCommentAsync(AddCommentDto addCommentDto);
    Task<Result<IEnumerable<CommentDto>>> GetCommentsByPlanIdAsync(string planId);
    Task<Result<CommentDto>> UpdateCommentAsync(UpdateCommentDto updateCommentDto);
    Task<Result<bool>> DeleteCommentAsync(string commentId, string userId);
}
