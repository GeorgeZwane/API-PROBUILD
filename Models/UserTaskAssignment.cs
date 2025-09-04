using ProBuild_Api.Models;
using System.ComponentModel.DataAnnotations;

namespace ProBuildWebAPI_v2_.Models
{
    public class UserTaskAssignment
    {
        [Key]
        public Guid AssignmentId { get; set; }
        public int UserId { get; set; }
      
        public Guid TaskEntityId { get; set; }
     
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        // Optional field to indicate role on the task
        public string? RoleOnTask { get; set; } // e.g., "Plumber", "Supervisor"
    }
}
