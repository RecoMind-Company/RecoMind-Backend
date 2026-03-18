using Core.Dtos;
using Core.Result;

namespace Core.ServicesAbstraction;

public interface ICommentService
{
    Task<Result<CommentDto>> AddCommentAsync(AddCommentDto addCommentDto);
}
