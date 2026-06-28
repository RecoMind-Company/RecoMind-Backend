namespace Core.Dtos.AI;

public class PostTasksDto
{
    public IEnumerable<AITasksDto> tasksDto { set; get; } = [];
    public string moduleId { set; get; }
    public string planId { set; get; }
}
