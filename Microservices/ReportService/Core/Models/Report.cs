namespace Core.Models;

public class Report
{
    public string Id { get; set; } = default!;
    public string TeamId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public Periodic Periodic { get; set; }
    public DateTime GeneratedDate { get; set; }
    public string FileType { get; set; } = default!;
    public string FilePath { get; set; } = default!;
}
