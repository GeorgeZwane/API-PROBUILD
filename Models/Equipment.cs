using System;
namespace ProBuildWebAPI_v2_.Models
{
    public class Equipment
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
            
        public required int Quantity { get; set; }

        public  required string Category { get; set; }
        public required string Condition { get; set; }


        public int? ProjectId { get; set; }

        public Project Project { get; set; } = null!;
    }
}
