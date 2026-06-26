namespace Core.Result;

public class ModuleErrors
{
    public static Error NotFound =>
       new("Module.NotFound", "The specified Module was not found.");
}
