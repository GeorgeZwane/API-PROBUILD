using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data;
using ProBuild_API.DTOs;
using ProBuild_API.Models;
using ProBuildWebAPI_v2_.Models;

namespace ProBuild_API.Controllers
{
    [Route("api/webprojects")]
    [ApiController]
    public class ProjectTeamController : Controller
    {
        private readonly ProBuildDbContext dbContext;

        public ProjectTeamController(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("CreateProjectTeam")]
        public IActionResult CreateProjectTeam(ProjectTeamAssignmentDTO projectTeamAssignmentDTO)
        {

            var equipmentEntity = new ProjectTeam
            {
                UserId = projectTeamAssignmentDTO.UserId,
                ProjectId = projectTeamAssignmentDTO.ProjectId,
                TeamName = projectTeamAssignmentDTO.TeamName,
                TeamCreationDate = projectTeamAssignmentDTO.TeamCreationDate,

            };

            dbContext.ProjectTeams.Add(equipmentEntity);
            dbContext.SaveChanges();
            return Ok(equipmentEntity);
        }

        [HttpGet("GetAllProjectTeams")]
        public IActionResult GetAllProjectTeams()
        {
            var teams = dbContext.ProjectTeams.ToList();
            return Ok(teams);
        }

        [HttpGet("GetTeamsByProjectId/{projectId}")]
        public IActionResult GetTeamsByProjectId(int projectId)
        {
            var teams = dbContext.ProjectTeams
                .Where(t => t.ProjectId == projectId)
                .ToList();

            return Ok(teams);
        }

        [HttpGet("GetTeamsByUserId/{userId}")]
        public IActionResult GetTeamsByUserId(int userId)
        {
            var teams = dbContext.ProjectTeams
                .Where(t => t.UserId == userId)
                .ToList();

            return Ok(teams);
        }

    }
}
