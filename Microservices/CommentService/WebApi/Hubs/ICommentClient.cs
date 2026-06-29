using Core.Dtos.Plan;
using Core.Result;

namespace WebApi.Hubs;

public interface ICommentClient
{
    Task ReceiveComment(PlanCommentDto comment);
    Task ReceiveEditedComment(PlanCommentDto comment);
    Task ReceiveErrors(IEnumerable<Error> errors);
    Task ReceiveDeletedComment(string commentId, string message);
}
