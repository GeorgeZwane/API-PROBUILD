using System;
using ProBuildWebAPI_v2_.Models;

namespace ProBuildWebAPI_v2_.Models
{
    public class Material
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required double Quantity { get; set; }
        public required string MetricUnit { get; set; }

        public required int ProjectId { get; set; }

        // Navigation property for the Project
        public Project Project { get; set; } = null!;
    }
}
