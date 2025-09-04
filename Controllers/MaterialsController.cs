using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using ProBuild_API.Data;
using ProBuild_API.DTOs;
using ProBuildWebAPI_v2_.DTOs;
using ProBuildWebAPI_v2_.Models;
using Task = ProBuildWebAPI_v2_.Models;


namespace ProBuildWebAPI_v2_.Controllers
{
    [Route("api/web/material")]
    [ApiController]
    public class MaterialController : ControllerBase
    {
        private readonly ProBuildDbContext dbContext;
        public MaterialController(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet("getAllMaterials")]
        public IActionResult GetAllMaterials()
        {
            var Alltasks = dbContext.Materials.ToList();

            return Ok(Alltasks);
        }

        [HttpPost("createMaterial")]
        public IActionResult CreateMaterial(AddMaterialsProjectDTO addMaterials)
        {
            var project = dbContext.Projects.FirstOrDefault(p => p.ProjectId == addMaterials.ProjectId);
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
            dbContext.Materials.Add(matEntity);
            dbContext.SaveChanges();
            return Ok(matEntity);
        }

       

    }
}
