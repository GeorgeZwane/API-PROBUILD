using System;
using System.ComponentModel.DataAnnotations;

namespace ProBuildWebAPI_v2_.DTOs
{
    public class AppAddProjectDTO
    {
        public int ProjectId { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "Project name must be at least 3 characters.")]
        public string Name { get; set; } = string.Empty; 

        [Required]
        public string Location { get; set; } = string.Empty; 

        [Required]
        [DataType(DataType.Date)]
        public DateTime Startdate { get; set; } 

        [Required]
        [DataType(DataType.Date)]
        public DateTime Enddate { get; set; } 

        public double? Progress { get; set; } 

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget must be a positive value.")]
        public double Budget { get; set; } 

        public string? Description { get; set; } 
    }
}
