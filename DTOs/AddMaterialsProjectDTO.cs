namespace ProBuild_API.DTOs
{
    public class AddMaterialsProjectDTO
    {
        public int ProjectId { get; set; }
        public Guid Id { get; set; } // Add this line for the ID
        public required string Name { get; set; }
        public required int Quantity { get; set; }
        public required string MetricUnit { get; set; }
      
    }
}
