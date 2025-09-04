using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data;
using ProBuild_API.DTOs;
using ProBuild_API.Models;
using ProBuildWebAPI_v2_.DTOs;
using ProBuildWebAPI_v2_.Models;


namespace ProBuildWebAPI_v2_.Controllers
{
    [Route("api/webprojects")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ProBuildDbContext dbContext;
        public ProjectController(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("getAllProjects")]
        public IActionResult GetAllProjects()
        {
            var allProjects = dbContext.Projects
                .Select(p => new AddProjectDTO
                {
                    ProjectId = p.ProjectId,
                    Name = p.Name,
                    Location = p.Location,
                    Startdate = p.Startdate,
                    Enddate = p.Enddate,
                    Progress = p.Progress,
                    Budget = p.Budget,
                    Description = p.Description,
                    Status=p.Status
                })
                .ToList();

            return Ok(allProjects);
        }

        [HttpGet("getAllProjectsByUserID")]
        public IActionResult GetAllProjects(int id)
        {
            var userProjects = dbContext.Projects
                .Where(p => p.UserId == id) 
                .Select(p => new AddProjectDTO
                {
                    ProjectId = p.ProjectId,
                    Name = p.Name,
                    Location = p.Location,
                    Startdate = p.Startdate,
                    Enddate = p.Enddate,
                    Progress = p.Progress,
                    Budget = p.Budget,
                    Description = p.Description,
                    UserId = p.UserId 
                })
                .ToList();

            return Ok(userProjects);
        }

        [HttpGet("getAllProjectsByStatus")]
        public IActionResult GetAllProjects(int id, string? status = null)
        {
            var query = dbContext.Projects
                .Where(p => p.UserId == id);

            // If status is provided, filter by it
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var userProjects = query
                .Select(p => new AddProjectDTO
                {
                    ProjectId = p.ProjectId,
                    Name = p.Name,
                    Location = p.Location,
                    Startdate = p.Startdate,
                    Enddate = p.Enddate,
                    Progress = p.Progress,
                    Budget = p.Budget,
                    Description = p.Description,
                    UserId = p.UserId
                })
                .ToList();

            return Ok(userProjects);
        }
        //GET /getAllProjectsByUserID?id=3&status=InProgress

        [HttpPost("createProjects")]
        public IActionResult CreateProject(AddProjectDTO addProjectDTO)
        {
           
            var projectEntity = new Project
            {
              
                Name = addProjectDTO.Name,
                Location = addProjectDTO.Location,
                Startdate = addProjectDTO.Startdate,
                Enddate = addProjectDTO.Enddate,
                Budget = addProjectDTO.Budget,
                Description = addProjectDTO.Description,
                Progress = 0.0,
                Status = "Not Started",
                UserId = addProjectDTO.UserId

            };

            dbContext.Projects.Add(projectEntity);
            dbContext.SaveChanges();

            //return Ok("Project Created Successfully");
            return Ok(new
            {
                projectId = projectEntity.ProjectId, 
                message = "Project Created Successfully"
            });
        }

        //[HttpDelete]
        //[Route("{name}")]
        //public IActionResult DeleteProject(string name)
        //{
        //    var pro = dbContext.Projects.FirstOrDefault(db => db.Name == name);

        //    if (pro == null)
        //    {
        //        return NotFound();
        //    }

        //    dbContext.Projects.Remove(pro);
        //    dbContext.SaveChanges();
        //    return Ok("Project deleted");
        //}

        [HttpGet("countAllProjectAndTasks")]
        public IActionResult GetProjectCount()
        {
            int totalProjects = dbContext.Projects.Count();
            var completeProjects = dbContext.Projects.Count(p => p.Status == "Completed");
            var inProgressProjects = dbContext.Projects.Count(p => p.Status == "In Progress");
            var InwaitingProjects = dbContext.Projects.Count(p => p.Status == "Not Started");

            var totalTasks = dbContext.Tasks.Count();

            var completeTasks = dbContext.Tasks.Count(t => t.Status == "Completed");
            var inProgressTasks = dbContext.Tasks.Count(t => t.Status == "In Progress");
            var inwatngTasks = dbContext.Tasks.Count(t => t.Status == "Not Started");
            var totalBudget = dbContext.Projects.Sum(p => (decimal?)p.Budget) ?? 0m;

            return Ok(new
            {
                ProjectCount = totalProjects,
                CompleteProjects = completeProjects,
                InProgressProjects = inProgressProjects,
                InWaitingProjects = InwaitingProjects,

                TotalTasks = totalTasks,
                CompleteTasks = completeTasks,
                InProgressTasks = inProgressTasks,
                InWaitingtasks = inwatngTasks,
                TotalBudget = totalBudget
        });
        }

        



        [HttpPut("updateProject/{id}")] // Use PUT for full updates
        public async Task<IActionResult> UpdateProject(int id, [FromBody] AddProjectDTO updateProjectDto)
        {
            if (id != updateProjectDto.ProjectId)
            {
                return BadRequest("Project ID in URL does not match Project ID in body.");
            }

            var existingProject = await dbContext.Projects.FindAsync(id);

            if (existingProject == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            // Update properties of the existing project
            existingProject.Name = updateProjectDto.Name;
            existingProject.Location = updateProjectDto.Location;
            existingProject.Startdate = updateProjectDto.Startdate;
            existingProject.Enddate = updateProjectDto.Enddate;
            existingProject.Progress = updateProjectDto.Progress;
            existingProject.Budget = updateProjectDto.Budget;
            existingProject.Description = updateProjectDto.Description;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound($"Project with ID {id} not found after concurrency check.");
                }
                else
                {
                    throw; // Re-throw if it's another concurrency issue
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"An error occurred while updating the project: {ex.Message}");
            }

            return NoContent(); // Return 204 No Content for successful updates
        }

        // Helper method to check if a project exists (used in concurrency check)
        private bool ProjectExists(int id)
        {
            return dbContext.Projects.Any(e => e.ProjectId == id);
        }



        [HttpDelete("deleteProject/{id}")] // Route for deleting a project, e.g., DELETE /Projects/123
        public async Task<IActionResult> DeleteProject(int id)
        {
            var projectToDelete = await dbContext.Projects.FindAsync(id);

            if (projectToDelete == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            try
            {
                dbContext.Projects.Remove(projectToDelete); // Mark the entity for removal
                await dbContext.SaveChangesAsync(); // Commit the deletion to the database
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"An error occurred while deleting the project: {ex.Message}");
            }

            return NoContent(); // Return 204 No Content for a successful deletion
        }

       

        }
    }
