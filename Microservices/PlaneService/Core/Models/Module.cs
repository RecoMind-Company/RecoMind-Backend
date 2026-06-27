namespace Core.Models;

public class Module
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string PlanId { get; set; }
    public Plan Plan { get; set; }
}
