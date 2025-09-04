namespace ProBuild_API.Models
{
    public class ProjectTeam
    {
        public Guid ProjectTeamId { get; set; }
        public required int UserId { get; set; }
        public required int ProjectId { get; set; }

        public String? TeamName { get; set; }
        public DateTime TeamCreationDate { get; set; } = DateTime.UtcNow;

    }
}
