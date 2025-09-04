using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data;
using ProBuild_API.DTOs;
using ProBuildWebAPI_v2_.DTOs; 
using ProBuildWebAPI_v2_.Models; 
using System.Security.Claims; 
using Microsoft.AspNetCore.Authorization;
using ProBuild_API.Models;

namespace ProBuildWebAPI_v2_.Controllers
{
    [Route("api/projects")]
    [ApiController]
    public class AppProjectController : ControllerBase
    {
        private readonly ProBuildDbContext dbContext;

        public AppProjectController(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Helper method to get the current authenticated user's ID
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


        [HttpGet("getProjectReport/{projectId}")]
        [Authorize]
        public async Task<ActionResult<ProjectReportDTO>> GetProjectReport(int projectId)
        {
            try
            {
                // Verify authenticated user
                var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(authenticatedUserId) || !int.TryParse(authenticatedUserId, out var parsedUserId))
                {
                    return StatusCode(403, new { error = "Unauthorized access." });
                }

                // Verify the project exists and belongs to the user
                var project = await dbContext.Projects
                    .Where(p => p.ProjectId == projectId && p.UserId == parsedUserId)
                    .FirstOrDefaultAsync();
                if (project == null)
                {
                    return NotFound(new { error = "Project not found or you do not have permission." });
                }

                // Calculate task counts
                var totalTasks = await dbContext.Tasks.CountAsync(t => t.ProjectId == projectId);
                var completedTasks = await dbContext.Tasks.CountAsync(t => t.ProjectId == projectId && t.Status == "Complete");
                var tasksInProgress = await dbContext.Tasks.CountAsync(t => t.ProjectId == projectId && t.Status == "In Progress");

                // Create response
                var report = new ProjectReportDTO
                {
                    ProjectName = project.Name,
                    StartDate = project.Startdate,
                    EndDate = project.Enddate,
                    TotalTasks = totalTasks,
                    CompletedTasks = completedTasks,
                    TasksInProgress = tasksInProgress,
                    Budget = project.Budget
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error fetching project report: {ex.Message}" });
            }
        }



        //[HttpGet("getProjectReport/{projectId}")]
        //public async Task<ActionResult> GetProjectReport(int projectId)
        //{
        //    // Verify the project exists
        //    var project = await dbContext.Projects.FindAsync(projectId);
        //    if (project == null)
        //    {
        //        return NotFound("Project not found.");
        //    }

        //    // Calculate task counts for the specific project
        //    var totalTasks = await dbContext.Tasks.CountAsync(t => t.ProjectId == projectId);
        //    var completedTasks = await dbContext.Tasks.CountAsync(t => t.ProjectId == projectId && t.Status == "Complete");
        //    var tasksInProgress = await dbContext.Tasks.CountAsync(t => t.ProjectId == projectId && t.Status == "In Progress");

        //    // Use the budget from the Project model
        //    var budget = project.Budget;

        //    return Ok(new
        //    {
        //        TotalTasks = totalTasks,
        //        CompletedTasks = completedTasks,
        //        TasksInProgress = tasksInProgress,
        //        Budget = budget,
        //    });

            
        //}



        [HttpGet("getProjectsByUserId/{userId}")]
        //[Authorize]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjectsByUserId(string userId)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { error = "User ID cannot be empty." });
            }

            if (!int.TryParse(userId, out var parsedUserId))
            {
                return BadRequest(new { error = "Invalid user ID format. Must be a valid integer." });
            }

            // Verify authenticated user ID from JWT token
            var authenticatedUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(authenticatedUserId) || authenticatedUserId != userId)
            {
                return StatusCode(403, new { error = "You do not have permission to access this data." });
            }
            try
            {
                // Query projects for the specified user
                var projects = await dbContext.Projects
                    .Where(p => p.UserId == parsedUserId)
                    .ToListAsync();

                if (projects == null || !projects.Any())
                {
                    return NotFound(new { error = "No projects found for this user." });
                }

                return Ok(projects);
            }
            catch (Exception ex)
            {
                // Log the exception (use your logging mechanism, e.g., Serilog, ILogger)
               // _logger.LogError(ex, "Error fetching projects for user {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while fetching projects." });
            }
        }



        [HttpGet("getAllProjects")] 
        [Authorize] 
        public IActionResult GetAllProjects()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized("User ID not found in token or invalid format. Please ensure you are authenticated.");
            }

            var allProjects = dbContext.Projects
                .Where(p => p.UserId == userId.Value) 
                .Select(p => new AppAddProjectDTO 
                {
                    ProjectId = p.ProjectId,
                    Name = p.Name,
                    Location = p.Location,
                    Startdate = p.Startdate,
                    Enddate = p.Enddate,
                    Progress = p.Progress,
                    Budget = p.Budget,
                    Description = p.Description
                })
                .ToList();

            return Ok(allProjects);
        }


        [HttpGet("countAllProjectAndTasks")]
        public IActionResult GetProjectsCount()
        {
            int totalProjects = dbContext.Projects.Count();
            var completeProjects = dbContext.Projects.Count(p => p.Status == "Complete");
            var inProgressProjects = dbContext.Projects.Count(p => p.Status == "In Progress");
            var incompleteProjects = dbContext.Projects.Count(p => p.Status == "Incomplete");

            var totalTasks = dbContext.Tasks.Count();

            var completeTasks = dbContext.Tasks.Count(t => t.Status == "Complete");
            var inProgressTasks = dbContext.Tasks.Count(t => t.Status == "In Progress");
            var incompleteTasks = dbContext.Tasks.Count(t => t.Status == "Incomplete");
            var totalBudget = dbContext.Projects.Sum(p => (decimal?)p.Budget) ?? 0m;

            return Ok(new
            {
                ProjectCount = totalProjects,
                CompleteProjects = completeProjects,
                InProgressProjects = inProgressProjects,
                IncompleteProjects = incompleteProjects,
                TotalTasks = totalTasks,
                CompleteTasks = completeTasks,
                InProgressTasks = inProgressTasks,
                IncompleteTasks = incompleteTasks,
                TotalBudget = totalBudget
            });
        }

        [HttpPost("createProjects")] 
        [Authorize] 
        public IActionResult CreateProject(AppAddProjectDTO addProjectDTO)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized("User ID not found in token or invalid format. Please ensure you are authenticated.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var projectEntity = new Project
            {
                Name = addProjectDTO.Name,
                Location = addProjectDTO.Location,
                Startdate = addProjectDTO.Startdate,
                Enddate = addProjectDTO.Enddate,
                Budget = addProjectDTO.Budget,
                Description = addProjectDTO.Description,
                Progress = addProjectDTO.Progress,
                UserId = userId.Value 
            };

            dbContext.Projects.Add(projectEntity);
            dbContext.SaveChanges(); // Consider async await dbContext.SaveChangesAsync();

            // Return the created project's ID and a message
            return Ok(new
            {
                projectId = projectEntity.ProjectId,
                message = "Project Created Successfully"
            });
        }


        [HttpGet("getProjectById/{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { error = "User ID not found in token or invalid format. Please ensure you are authenticated." });
            }

            var project = await dbContext.Projects
                                    .Where(p => p.ProjectId == id && p.UserId == userId.Value)
                                    .Select(p => new Project                                     {
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
                                    .FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound($"Project with ID {id} not found or you do not have access to it.");
            }

            return Ok(project);
        }



        [HttpGet("count")] 
        [Authorize] 
        public IActionResult GetProjectCount()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized("User ID not found in token or invalid format. Please ensure you are authenticated.");
            }

            int count = dbContext.Projects.Where(p => p.UserId == userId.Value).Count(); 
            return Ok(new { ProjectCount = count });
        }






        [HttpPut("updateProject/{id}")] 
        [Authorize] 
        public async Task<IActionResult> UpdateProject(int id, [FromBody] AppAddProjectDTO updateProjectDto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized("User ID not found in token or invalid format. Please ensure you are authenticated.");
            }

            if (id != updateProjectDto.ProjectId)
            {
                return BadRequest("Project ID in URL does not match Project ID in body.");
            }

            var existingProject = await dbContext.Projects.FindAsync(id);

            if (existingProject == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            // Authorization check: Ensure the authenticated user owns this project
            if (existingProject.UserId != userId.Value)
            {
                return Forbid("You do not have permission to update this project."); 
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
                    throw; 
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the project: {ex.Message}");
            }

            return NoContent();
        }

        // Helper method to check if a project exists
        private bool ProjectExists(int id)
        {
            return dbContext.Projects.Any(e => e.ProjectId == id);
        }


        [HttpDelete("deleteProject/{id}")] 
        [Authorize] 
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized("User ID not found in token or invalid format. Please ensure you are authenticated.");
            }

            var projectToDelete = await dbContext.Projects.FindAsync(id);

            if (projectToDelete == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            if (projectToDelete.UserId != userId.Value)
            {
                return Forbid("You do not have permission to delete this project."); 
            }

            try
            {
                dbContext.Projects.Remove(projectToDelete); 
                await dbContext.SaveChangesAsync(); // Commit the deletion to the database
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the project: {ex.Message}");
            }

            return NoContent();
        }
    }
}
