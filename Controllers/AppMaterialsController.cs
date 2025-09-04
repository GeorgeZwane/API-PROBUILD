using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data;
using ProBuild_API.DTOs;
using ProBuildWebAPI_v2_.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProBuild_API.Controllers
{
    [ApiController]
    [Route("api/materials")]
    public class AppMaterialsController : ControllerBase
    {
        private readonly ProBuildDbContext _dbContext;

        public AppMaterialsController(ProBuildDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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

        [HttpPost("addMaterial")]
        [Authorize]
        public IActionResult AddMaterial(AddMaterialDTO addMaterials)
        {
            var project = _dbContext.Projects.FirstOrDefault(p => p.ProjectId == addMaterials.ProjectId);
            if (project == null)
            {
                return NotFound($"Project with ID {addMaterials.ProjectId} not found.");
            }

            var matEntity = new Material()
            {
                Name = addMaterials.Name,
                Quantity = addMaterials.Quantity,
                MetricUnit = addMaterials.MetricUnit,
                ProjectId = addMaterials.ProjectId
            };
            _dbContext.Materials.Add(matEntity);

            _dbContext.SaveChanges();
            return Ok(matEntity);
        }

        [HttpGet("getAllMaterials")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Material>>> GetAllMaterials()
        {
            var allMaterials = _dbContext.Materials
                .Select(e => new AddMaterialDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    Quantity = e.Quantity,
                    MetricUnit = e.MetricUnit,
                    ProjectId = e.ProjectId
                })
                .ToList();

            return Ok(allMaterials);
        }

        //[HttpGet("getAllMaterials")]
        //[Authorize]
        //public IActionResult GetAllMaterials()
        //{
        //    var AllMaterials = _dbContext.Materials.ToList();

        //    return Ok(AllMaterials);
        //}



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
        public IActionResult GetMaterialByProjectId(int projectId)
        {
            var projectExists = _dbContext.Projects.Any(p => p.ProjectId == projectId);
            if (!projectExists)
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            var MaterialForProject = _dbContext.Materials
                .Where(e => e.ProjectId == projectId)
                .Select(e => new AddMaterialDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    Quantity = (int)e.Quantity,
                    MetricUnit = e.MetricUnit,
                    ProjectId = e.ProjectId
                })
                .ToList();

            return Ok(MaterialForProject);
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
