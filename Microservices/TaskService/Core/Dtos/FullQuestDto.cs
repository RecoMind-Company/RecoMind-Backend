namespace Core.Dtos;

public class FullQuestDto
{
    public QuestDto questDto { get; set; } = default!;
    public List<string> UserIds { get; set; } = [];
}
