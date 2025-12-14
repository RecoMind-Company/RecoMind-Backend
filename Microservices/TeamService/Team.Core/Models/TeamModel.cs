using System.ComponentModel.DataAnnotations;

namespace Team.Core.Models
{
    public class TeamModel
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string CompanyId { get; set; }
        public string? TeamLeadId { get; set; }
        public List<TeamEmployee> TeamEmployees { get; set; } = new();
    }
}
