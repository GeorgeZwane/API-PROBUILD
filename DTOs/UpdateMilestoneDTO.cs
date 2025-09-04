namespace ProBuild_API.DTOs
{
    public class UpdateMilestoneDTO
    {
        public Guid Id { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
    }
}
