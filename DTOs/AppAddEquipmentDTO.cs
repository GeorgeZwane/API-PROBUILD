using System;

namespace ProBuild_API.DTOs
{
    public class AppAddEquipmentDTO
    {
        public Guid Id { get; set; } 
        public required string Name { get; set; }
        public required int Quantity { get; set; }
        public required string Category { get; set; }
        public required string Condition { get; set; }

        // ProjectId to link equipment to a project
        public  int? ProjectId { get; set; }
    }
}
