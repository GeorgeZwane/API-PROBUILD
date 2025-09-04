namespace ProBuild_API.DTOs
{
    public class MileStoneDTO
    {
        public Guid Id { get; set; }
        public required string MilestoneName { get; set; }
        public required string Description { get; set; }
        public required DateTime DueDate { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }

        // Link to Task
        public Guid TaskEntityId { get; set; }

        public DateTime MilestoneStartDate { get; set; } = DateTime.UtcNow;
    }
}
