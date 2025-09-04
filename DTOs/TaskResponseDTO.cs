using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ProBuild_API.DTOs
{
    public class TaskResponseDTO
    {
        public int Id { get; set; }
       
        public required string TaskName { get; set; }
        public required string Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }
        public string? Priority { get; set; }

        public double? Progress { get; set; }

        public int ProjectId { get; set; }

        //public int AssignedUserId { get; set; }
        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();

    }
}
