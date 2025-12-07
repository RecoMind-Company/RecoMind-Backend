namespace Core.DTOs
{
    public class GetCompanyDTO
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Industry { get; set; }
        public string? Country { get; set; }
        public string? AdminId { get; set; }
        public string? SubscriptionId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
