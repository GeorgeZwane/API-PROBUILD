using System;

namespace ProBuild_API.DTOs
{
    // DTO for adding a new material.
    public class AddMaterialDTO
    {
        public required string Name { get; set; }
        public required double Quantity { get; set; }
        public required string MetricUnit { get; set; }
        public required int ProjectId { get; set; }
    }

    // DTO for updating an existing material.
    public class UpdateMaterialDTO
    {
        public string? Name { get; set; }
        public double? Quantity { get; set; }
        public string? MetricUnit { get; set; }
        public int? ProjectId { get; set; }
    }
}
