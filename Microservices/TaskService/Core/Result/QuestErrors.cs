namespace Core.Result;

public static class QuestErrors
{
    public static Error QuestNotFound =>
        new("Task.NotFound", "The specified Task was not found.");
}
