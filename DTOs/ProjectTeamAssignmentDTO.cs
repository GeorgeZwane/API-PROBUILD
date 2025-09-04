namespace ProBuild_API.DTOs
{
    public class ProjectTeamAssignmentDTO
    {
        public required int UserId { get; set; }
        public required int ProjectId { get; set; }

        public String? TeamName { get; set; }
        public DateTime TeamCreationDate { get; set; } = DateTime.UtcNow;
    }
}
