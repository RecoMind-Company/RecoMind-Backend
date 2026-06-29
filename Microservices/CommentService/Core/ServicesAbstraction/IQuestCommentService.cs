using Core.Dtos.Task;
using Core.Result;

namespace Core.ServicesAbstraction;

public interface IQuestCommentService
{
    Task<Result<TaskCommentDto>> AddCommentAsync(AddTaskCommentDto addCommentDto);
    Task<Result<IEnumerable<TaskCommentDto>>> GetCommentsByTaskIdAsync(string taskId);
    Task<Result<TaskCommentDto>> UpdateCommentAsync(UpdateTaskCommentDto updateCommentDto);
    Task<Result<bool>> DeleteCommentAsync(string commentId, string userId);
}
