using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data;
using ProBuild_API.DTOs;
using ProBuildWebAPI_v2_.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;

namespace ProBuild_API.Controllers
{
    [ApiController]
    [Route("api/materials")]
    public class MaterialController : ControllerBase
    {
        private readonly ProBuildDbContext _dbContext;

        public MaterialController(ProBuildDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("getAllMaterials")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Material>>> GetAllMaterials()
        {
            var materials = await _dbContext.Materials.ToListAsync();

            if (!materials.Any())
            {
                return NotFound("No materials found in the database.");
            }

            return Ok(materials);
        }


        // Retrieves a single material by its unique ID.
        [HttpGet("getMaterialById/{id}")]
        [Authorize]
        public async Task<ActionResult<Material>> GetMaterialById(Guid id)
        {
            var material = await _dbContext.Materials.FindAsync(id);

            if (material == null)
            {
                return NotFound($"Material with ID {id} not found.");
            }

            return Ok(material);
        }


        [HttpGet("byProject/{projectId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Material>>> GetMaterialsByProjectId(int projectId)
        {
            var materials = await _dbContext.Materials
                .Where(m => m.ProjectId == projectId)
                .ToListAsync();

            if (!materials.Any())
            {
                return NotFound($"No materials found for project ID {projectId}.");
            }

            return Ok(materials);
        }

        [HttpPost("addMaterial")]
        [Authorize]
        public async Task<IActionResult> AddMaterial(AddMaterialDTO addMaterialDto)
        {
            // First, check if the project exists.
            var projectExists = await _dbContext.Projects.AnyAsync(p => p.ProjectId == addMaterialDto.ProjectId);
            if (!projectExists)
            {
                return BadRequest($"Project with ID {addMaterialDto.ProjectId} not found.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var materialEntity = new Material
            {
                Id = Guid.NewGuid(),
                Name = addMaterialDto.Name,
                Quantity = addMaterialDto.Quantity,
                MetricUnit = addMaterialDto.MetricUnit,
                ProjectId = addMaterialDto.ProjectId
            };

            await _dbContext.Materials.AddAsync(materialEntity);
            await _dbContext.SaveChangesAsync();

            return Ok(new { MaterialId = materialEntity.Id, Message = "Material successfully added." });
        }

        [HttpPut("updateMaterial/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateMaterial(Guid id, UpdateMaterialDTO updateMaterialDto)
        {
            var existingMaterial = await _dbContext.Materials.FindAsync(id);
            if (existingMaterial == null)
            {
                return NotFound($"Material with ID {id} not found.");
            }

            // Update properties from the DTO.
            existingMaterial.Name = updateMaterialDto.Name ?? existingMaterial.Name;
            existingMaterial.Quantity = updateMaterialDto.Quantity ?? existingMaterial.Quantity;
            existingMaterial.MetricUnit = updateMaterialDto.MetricUnit ?? existingMaterial.MetricUnit;
            existingMaterial.ProjectId = updateMaterialDto.ProjectId ?? existingMaterial.ProjectId;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _dbContext.Materials.AnyAsync(m => m.Id == id))
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

    }
}
