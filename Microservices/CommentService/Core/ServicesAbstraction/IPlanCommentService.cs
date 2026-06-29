using Core.Dtos.Plan;
using Core.Result;

namespace Core.ServicesAbstraction;

public interface IPlanCommentService
{
    Task<Result<PlanCommentDto>> AddCommentAsync(AddPlanCommentDto addCommentDto);
    Task<Result<IEnumerable<PlanCommentDto>>> GetCommentsByPlanIdAsync(string planId);
    Task<Result<PlanCommentDto>> UpdateCommentAsync(UpdatePlanCommentDto updateCommentDto);
    Task<Result<bool>> DeleteCommentAsync(string commentId, string userId);
}
