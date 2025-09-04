using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data; // Assuming your DbContext is here
using ProBuildWebAPI_v2_.Models; // Assuming your Equipment model is here
using ProBuild_API.DTOs; // Assuming your AddEquipmentDTO is here
using System.Linq; // Needed for .ToList()

namespace ProBuild_API.Controllers
{
    [ApiController] // Add this if not already present, good practice for API controllers
    [Route("api/webequipments")] // Makes the base route /Equipment
    public class EquipmentController : ControllerBase // Change from Controller to ControllerBase for API
    {
        private readonly ProBuildDbContext dbContext; // Replace ProBuildDbContext with your actual DbContext class name

        public EquipmentController(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("addEquipment")] // Explicitly define route for clarity
        public IActionResult AddEquipment(AddEquipmentDTO addEquipmentDTO) // Renamed parameter for clarity
        {
            // Create Equipment entity from DTO
            var equipmentEntity = new Equipment
            {
                Id = Guid.NewGuid(), // Generate a new GUID for the ID
                Name = addEquipmentDTO.Name,
                Quantity = addEquipmentDTO.Quantity,
                Category = addEquipmentDTO.Category,
                Condition = addEquipmentDTO.Condition,
            };

            dbContext.Equipments.Add(equipmentEntity);
            dbContext.SaveChanges(); // Synchronous save for simplicity, consider async in production

            // Return the created equipment with its generated ID
            return Ok(new { EquipmentId = equipmentEntity.Id, Message = "Equipment Successfully added" });
        }

        [HttpGet("count")]
        public IActionResult GetEquipmentCount()
        {
            int count = dbContext.Equipments.Count();
            return Ok(new { EquipmentCount = count }); // Changed ProjectCount to EquipmentCount
        }

        [HttpGet("getAllEquipments")] // New endpoint to get all equipment
        public IActionResult GetAllEquipment()
        {
            var allEquipment = dbContext.Equipments
                .Select(e => new AddEquipmentDTO // Using AddEquipmentDTO for consistency, assuming it has all fields
                {
                    // Note: AddEquipmentDTO doesn't have Id. If you need Id on Flutter,
                    // you'll need a new DTO or modify AddEquipmentDTO to include Id.
                    // For now, let's assume you'll update AddEquipmentDTO in C#
                    // or create a new DTO like EquipmentResponseDTO if Id is needed.
                    // For this example, I'll assume you'll add Id to AddEquipmentDTO if needed.
                    Name = e.Name,
                    Quantity = e.Quantity,
                    Category = e.Category,
                    Condition = e.Condition
                })
                .ToList();

            return Ok(allEquipment);
        }
    }
}
