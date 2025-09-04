using System;
using System.ComponentModel.DataAnnotations;
using ProBuild_API.Models;
using ProBuildWebAPI_v2_.Models; 

namespace ProBuildWebAPI_v2_.Models
{
    public class TaskEntity
    {
        [Key] 
        public Guid Id { get; set; } 

        public required string Name { get; set; } 

        public string? Description { get; set; } 

        public required DateTime Startdate { get; set; }
        public required DateTime Enddate { get; set; }  

        public required string Priority { get; set; } 
        public string? AssignedTo { get; set; } 

        public double? Progress { get; set; }
        public string? Status { get; set; }

        public int ProjectId { get; set; }
        public int UserId { get; set; }

    }
}
