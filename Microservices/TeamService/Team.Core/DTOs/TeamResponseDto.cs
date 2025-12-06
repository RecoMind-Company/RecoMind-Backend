namespace Team.Core.DTOs
{
    public class TeamResponseDto
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Name { get; set; }
        public string TeamLeadName { get; set; }
        public List<string> Employees { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
