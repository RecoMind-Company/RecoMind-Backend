namespace Core.Result;

public static class PlanErrors
{
    public static Error NotFound =>
        new("Plan.NotFound", "The specified Plan was not found.");
}
