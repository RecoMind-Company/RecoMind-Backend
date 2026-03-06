namespace Core.Result;

public static class QuestErrors
{
    public static Error QuestNotFound =>
        new("Task.NotFound", "The specified Task was not found.");
    public static Error UserAlreadyAssignedToQuest =>
        new("Task.UserAlreadyAssigned", "The user is already assigned to this Task.");
}
