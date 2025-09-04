namespace ProBuild_API.DTOs
{
    public class AppProjectTeamAssignmentDTO
    {
        public required int UserId { get; set; }
        public required int ProjectId { get; set; }

        public String? TeamName { get; set; }
        public DateTime TeamCreationDate { get; set; } = DateTime.UtcNow;
    }
}
