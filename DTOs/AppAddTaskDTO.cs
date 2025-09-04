using System; 

namespace ProBuild_API.DTOs
{
    public class AppAddTaskDTO
    {
        public Guid Id { get; set; }

        public required string Name { get; set; } 

        public string? Description { get; set; } 

        public required DateTime Startdate { get; set; } 
        public required DateTime Enddate { get; set; }   

        public required string Priority { get; set; }    
        public required string AssignedTo { get; set; }  

        public double? Progress { get; set; }

        public required int ProjectId { get; set; }    
    }
}
