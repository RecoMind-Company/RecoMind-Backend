namespace Core.DTOs.Quest;

public class PostTasksDto
{
    public string PlanId { get; set; }
    public IEnumerable<PostModuleTasksDto> ModulesTasks { get; set; } = [];
}
