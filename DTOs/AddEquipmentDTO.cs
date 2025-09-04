namespace ProBuild_API.DTOs
{
    public class AddEquipmentDTO
    {

        public Guid Id { get; set; } // Add this line for the ID
        public required string Name { get; set; }
        public required int Quantity { get; set; }
        public required string Category { get; set; }
        public required string Condition { get; set; }
    }
}
