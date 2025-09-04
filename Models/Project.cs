using ProBuild_Api.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // For [Key]

namespace ProBuildWebAPI_v2_.Models
{
    public class Project
    {
        [Key] // Explicitly define ProjectId as the primary key
        public int ProjectId { get; set; }

        public required string Name { get; set; }

        public required string Location { get; set; }

        public required DateTime Startdate { get; set; }

        public required DateTime Enddate { get; set; }

        public double? Progress { get; set; }
        public string? Status { get; set; }


        
        public required double Budget { get; set; }

        public string? Description { get; set; } 

        public int UserId { get; set; } 
        public User User { get; set; } = null!; 

        // Navigation property for related Equipment
        public ICollection<Equipment> Equipment { get; set; } = new List<Equipment>(); 

        // Navigation property for related Tasks
        public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
    }
}
