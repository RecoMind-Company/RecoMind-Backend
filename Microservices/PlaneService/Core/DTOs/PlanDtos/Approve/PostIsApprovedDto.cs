namespace Core.DTOs.PlanDtos.Approve
{
    public class PostIsApprovedDto
    {
        public string PlanId { get; set; }
        public bool IsAproved { get; set; }
        public string? Feedback { get; set; }
        public string Status { get; set; }
    }
}
