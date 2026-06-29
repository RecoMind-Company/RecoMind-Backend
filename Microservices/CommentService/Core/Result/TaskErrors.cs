namespace Core.Result;

public class TaskErrors
{
    public static Error TaskNotFound => new("Comment.TaskNotFound", "This task isn't found");
    public static Error UserNotAssignedToTask => new("Comment.UserNotAssignedToTask", "this user is not assigned to this task");
}
