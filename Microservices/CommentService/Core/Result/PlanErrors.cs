namespace Core.Result;

public static class PlanErrors
{
    internal static Error PlanNotFound => new("Comment.PlanNotFound", "Plan isn't found");
}
