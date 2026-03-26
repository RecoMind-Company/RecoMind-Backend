namespace Core.Dtos;

public class CommentDto
{
    public string Id { get; set; } = default!;
    public string UserComment { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsUpdated => UpdatedAt.HasValue;
}
