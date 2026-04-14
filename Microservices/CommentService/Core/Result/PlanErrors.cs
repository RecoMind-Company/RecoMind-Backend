namespace Core.Result;

public static class PlanErrors
{
    public static Error PlanNotFound => new("Comment.PlanNotFound", "Plan isn't found");
}
