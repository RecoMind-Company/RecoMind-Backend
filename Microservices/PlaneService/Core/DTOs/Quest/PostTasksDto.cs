using Core.DTOs.AI;

namespace Core.DTOs.Quest;

public class PostTasksDto
{
    public IEnumerable<AITasksDto> tasksDto { set; get; } = [];
    public string moduleId { set; get; }
}
