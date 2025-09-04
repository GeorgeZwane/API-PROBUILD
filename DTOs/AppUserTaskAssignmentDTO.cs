namespace ProBuildWebAPI_v2_.Models
{
    public class AppUserTaskAssignmentDTO
    {
        public required Guid UserId { get; set; }
        public required Guid TaskEntityId { get; set; }
        public string? RoleOnTask { get; set; }
    }
}
