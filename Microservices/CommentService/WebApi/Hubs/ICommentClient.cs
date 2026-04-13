using Core.Dtos;
using Core.Result;

namespace WebApi.Hubs;

public interface ICommentClient
{
    Task ReceiveComment(CommentDto comment);
    Task ReceiveEditedComment(CommentDto comment);
    Task ReceiveErrors(IEnumerable<Error> errors);
    Task ReceiveDeletedComment(string commentId, string message);
}
