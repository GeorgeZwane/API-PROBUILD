namespace ProBuildWebAPI_v2_.Models
{
    public class UserTaskAssignmentDTO
    {
        public required int UserId { get; set; }
        public required Guid TaskEntityId { get; set; }
        public string? RoleOnTask { get; set; }
    }
}
