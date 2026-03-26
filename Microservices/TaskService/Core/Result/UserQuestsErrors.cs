namespace Core.Result;

public class UserQuestsErrors
{
    public static Error UserAlreadyAssignedToQuest =>
       new("Task.UserAlreadyAssigned", "The user is already assigned to this Task.");
    public static Error UserNotAssignedToQuest =>
        new("Task.UserNotAssigned", "This user is not assigned to this Task.");
}
