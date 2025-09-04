using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data;
using ProBuild_API.DTOs; 
using ProBuildWebAPI_v2_.DTOs; 
using ProBuildWebAPI_v2_.Models; 

namespace ProBuildWebAPI_v2_.Controllers
{
    [Route("api/tasks")] 
    [ApiController]
    //[Authorize] 
    public class AppTaskController : ControllerBase
    {
        private readonly ProBuildDbContext dbContext;

        public AppTaskController(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Helper method to get current authenticated user's ID
        private int? GetCurrentUserId()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
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

        [HttpGet("getTaskByProjectId/{projectId}")] 
        public async Task<IActionResult> GetTasksByProjectId(int projectId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized("User ID not found in token or invalid format. Please ensure you are authenticated.");
            }

            var project = await dbContext.Projects
                                         .Include(p => p.Tasks) 
                                         .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.UserId == userId.Value);

            if (project == null)
            {
                return NotFound($"Project with ID {projectId} not found or you do not have access.");
            }

            var tasks = project.Tasks.Select(t => new AppTaskResponseDTO
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                Name = t.Name,
                Description = t.Description,
                Startdate = t.Startdate,
                Enddate = t.Enddate,
                Progress = t.Progress,
                Priority = t.Priority,
                AssignedTo = t.AssignedTo 
            }).ToList();

            return Ok(tasks);
        }

        [HttpGet("countTasksByProject/{projectId}")]
        public IActionResult CountTasksByProjectId(int projectId)
        {
            bool projectExists = dbContext.Tasks.Any(p => p.ProjectId == projectId);
            if (!projectExists)
            {
                return NotFound(new { Message = $"Project with ID {projectId} not found." });
            }

            var totalTasks = dbContext.Tasks.Count(t => t.ProjectId == projectId);
            var completeTasks = dbContext.Tasks.Count(t => t.ProjectId == projectId && t.Status == "Complete");
            var inProgressTasks = dbContext.Tasks.Count(t => t.ProjectId == projectId && t.Status == "In Progress");
            var incompleteTasks = dbContext.Tasks.Count(t => t.ProjectId == projectId && t.Status == "Incomplete");

            return Ok(new
            {
                TotalTasks = totalTasks,
                CompleteTasks = completeTasks,
                InProgressTasks = inProgressTasks,
                IncompleteTasks = incompleteTasks,

            });
        }


        [HttpGet("getTaskById/{id:guid}")] 
        public async Task<IActionResult> GetTaskById(Guid id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized("User ID not found in token or invalid format. Please ensure you are authenticated.");
            }

            var task = await dbContext.Tasks
                                      .Include(t => t.ProjectId) 
                                      .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound($"Task with ID {id} not found.");
            }

            // Check if the task project belongs to the current user
            if (task.ProjectId != userId.Value)
            {
                return Forbid("You do not have permission to access this task.");
            }

            var taskResponse = new AppTaskResponseDTO
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                Startdate = task.Startdate,
                Enddate = task.Enddate,
                Progress = task.Progress,
                Priority = task.Priority,
                AssignedTo = task.AssignedTo
            };

            return Ok(taskResponse);
        }

        [HttpPost("createTask")] 
        public async Task<IActionResult> AppCreateTask(AppAddTaskDTO addTaskDTO)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized("User ID not found in token or invalid format. Please ensure you are authenticated.");
            }

            // Validate if the project exists and belongs to the current user
            var project = await dbContext.Projects
                                         .FirstOrDefaultAsync(p => p.ProjectId == addTaskDTO.ProjectId && p.UserId == userId.Value);

            if (project == null)
            {
                return NotFound($"Project with ID {addTaskDTO.ProjectId} not found or you do not have permission to add tasks to it.");
            }

            var taskEntity = new TaskEntity
            {
                Id = Guid.NewGuid(),
                Name = addTaskDTO.Name, 
                Description = addTaskDTO.Description,
                Startdate = addTaskDTO.Startdate,
                Enddate = addTaskDTO.Enddate,
                Progress = addTaskDTO.Progress, 
                Priority = addTaskDTO.Priority, 
                AssignedTo = addTaskDTO.AssignedTo,
                ProjectId = addTaskDTO.ProjectId,
            };

            dbContext.Tasks.Add(taskEntity);
            await dbContext.SaveChangesAsync(); // Use async SaveChanges

            return Ok(new AppTaskResponseDTO
            {
                Id = taskEntity.Id,
                Name = taskEntity.Name,
                Description = taskEntity.Description,
                Startdate = taskEntity.Startdate,
                Enddate = taskEntity.Enddate,
                Progress = taskEntity.Progress,
                Priority = taskEntity.Priority,
                AssignedTo = taskEntity.AssignedTo
            });
        }

        [HttpPut("updateTask/{id:guid}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] AppAddTaskDTO updateTaskDTO)
        {
            if (id != updateTaskDTO.Id) 
            {
                return BadRequest("Task ID in URL does not match Task ID in body.");
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized("User ID not found in token or invalid format. Please ensure you are authenticated.");
            }

            var existingTask = await dbContext.Tasks
                                              .Include(t => t.ProjectId) 
                                              .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTask == null)
            {
                return NotFound($"Task with ID {id} not found.");
            }

            if (existingTask.ProjectId != userId.Value)
            {
                return Forbid("You do not have permission to update this task.");
            }

            existingTask.Name = updateTaskDTO.Name;
            existingTask.Description = updateTaskDTO.Description;
            existingTask.Startdate = updateTaskDTO.Startdate;
            existingTask.Enddate = updateTaskDTO.Enddate;
            existingTask.Progress = updateTaskDTO.Progress;
            existingTask.Priority = updateTaskDTO.Priority;
            existingTask.AssignedTo = updateTaskDTO.AssignedTo;
            // ProjectId should not be changeable via update in this context
            // existingTask.ProjectId = updateTaskDTO.ProjectId; 

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await dbContext.Tasks.AnyAsync(t => t.Id == id))
                {
                    return NotFound($"Task with ID {id} not found after concurrency check.");
                }
                else
                {
                    throw; 
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the task: {ex.Message}");
            }

            return NoContent(); 
        }

        [HttpDelete("deleteTask/{id:guid}")] 
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized("User ID not found in token or invalid format. Please ensure you are authenticated.");
            }

            var taskToDelete = await dbContext.Tasks
                                              .Include(t => t.ProjectId) 
                                              .FirstOrDefaultAsync(t => t.Id == id);

            if (taskToDelete == null)
            {
                return NotFound($"Task with ID {id} not found.");
            }

            if (taskToDelete.ProjectId != userId.Value)
            {
                return Forbid("You do not have permission to delete this task.");
            }

            try
            {
                dbContext.Tasks.Remove(taskToDelete);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting the task: {ex.Message}");
            }

            return NoContent(); 
        }
    }

    



}
