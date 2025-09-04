using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data; 
using ProBuildWebAPI_v2_.Models;
using ProBuild_API.DTOs;
using System.Linq; 
using System.Security.Claims; 
using Microsoft.AspNetCore.Authorization;

namespace ProBuild_API.Controllers
{
    [ApiController]
    [Route("api/equipments")] 
    public class AppEquipmentController : ControllerBase
    {
        private readonly ProBuildDbContext dbContext;

        public AppEquipmentController(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Helper method to get the current authenticated user's ID
        // (Optional for Equipment if not linking to user directly, but useful for broader auth checks)
        private int? GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
            {
                return null;
            }
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            return null;
        }

        [HttpPost("addEquipment")] 
        [Authorize] 
        public IActionResult AddEquipment(AppAddEquipmentDTO addEquipmentDTO)
        {
            // Before creating, check if the ProjectId exists
            var projectExists = dbContext.Projects.Any(p => p.ProjectId == addEquipmentDTO.ProjectId);
            if (!projectExists)
            {
                return BadRequest($"Project with ID {addEquipmentDTO.ProjectId} not found.");
            }

         
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var equipmentEntity = new Equipment
            {
                Id = Guid.NewGuid(), 
                Name = addEquipmentDTO.Name,
                Quantity = addEquipmentDTO.Quantity,
                Category = addEquipmentDTO.Category,
                Condition = addEquipmentDTO.Condition,
                ProjectId = addEquipmentDTO.ProjectId 
            };

            dbContext.Equipments.Add(equipmentEntity);
            dbContext.SaveChanges(); // Consider async await dbContext.SaveChangesAsync();

            // Return the created equipment with its generated ID and ProjectId
            return Ok(new { EquipmentId = equipmentEntity.Id, ProjectId = equipmentEntity.ProjectId, Message = "Equipment Successfully added" });
        }

        [HttpGet("count")] 
        [Authorize]
        public IActionResult GetEquipmentCount()
        {
            int count = dbContext.Equipments.Count();
            return Ok(new { EquipmentCount = count });
        }

        [HttpGet("getAllEquipments")] 
        [Authorize]
        public IActionResult GetAllEquipment()
        {
            var allEquipment = dbContext.Equipments
                .Select(e => new AppAddEquipmentDTO 
                {
                    Id = e.Id,
                    Name = e.Name,
                    Quantity = e.Quantity,
                    Category = e.Category,
                    Condition = e.Condition,
                    ProjectId = e.ProjectId 
                })
                .ToList();

            return Ok(allEquipment);
        }

        [HttpGet("byProject/{projectId}")] 
        [Authorize]
        public IActionResult GetEquipmentByProjectId(int projectId)
        {
            var projectExists = dbContext.Projects.Any(p => p.ProjectId == projectId);
            if (!projectExists)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            var equipmentForProject = dbContext.Equipments
                .Where(e => e.ProjectId == projectId)
                .Select(e => new AppAddEquipmentDTO 
                {
                    Id = e.Id,
                    Name = e.Name,
                    Quantity = e.Quantity,
                    Category = e.Category,
                    Condition = e.Condition,
                    ProjectId = e.ProjectId
                })
                .ToList();

            return Ok(equipmentForProject);
        }


        // Placeholder for UpdateEquipment method 
        
        [HttpPut("updateEquipment/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEquipment(Guid id, [FromBody] AppAddEquipmentDTO updateEquipmentDTO)
        {
            var existingEquipment = await dbContext.Equipments.FindAsync(id);
            if (existingEquipment == null)
            {
                return NotFound($"Equipment with ID {id} not found.");
            }

            // Optional: Check if the project ID being updated matches the existing one,
            // or if a user is trying to move equipment between projects they don't own.
            var projectExists = dbContext.Projects.Any(p => p.ProjectId == updateEquipmentDTO.ProjectId);
            if (!projectExists) return BadRequest("Target Project not found.");
            //existingEquipment.ProjectId = updateEquipmentDTO.ProjectId; // If allowed to change project

            existingEquipment.Name = updateEquipmentDTO.Name;
            existingEquipment.Quantity = updateEquipmentDTO.Quantity;
            existingEquipment.Category = updateEquipmentDTO.Category;
            existingEquipment.Condition = updateEquipmentDTO.Condition;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!dbContext.Equipments.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }
        

        // Placeholder for DeleteEquipment method 
        /*
        [HttpDelete("deleteEquipment/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEquipment(Guid id)
        {
            var equipmentToDelete = await dbContext.Equipments.FindAsync(id);
            if (equipmentToDelete == null)
            {
                return NotFound($"Equipment with ID {id} not found.");
            }

            dbContext.Equipments.Remove(equipmentToDelete);
            await dbContext.SaveChangesAsync();
            return NoContent();
        }
        */
    }
}
