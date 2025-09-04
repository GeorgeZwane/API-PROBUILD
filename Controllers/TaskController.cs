using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data;
using ProBuild_API.DTOs;
using ProBuild_API.Service;
using ProBuildWebAPI_v2_.DTOs;
using ProBuildWebAPI_v2_.Models;
using Task = ProBuildWebAPI_v2_.Models;


namespace ProBuildWebAPI_v2_.Controllers
{

    [Route("api/webtask")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ProgressService _progressService;
        private readonly ProBuildDbContext dbContext;
        public TaskController(ProBuildDbContext dbContext, ProgressService progressService)
        {
            this._progressService = progressService;
            this.dbContext = dbContext;
        }
        [HttpGet("getAllTask")]
        public IActionResult GetAllTask()
        {
            var Alltasks = dbContext.Tasks.ToList();

            return Ok(Alltasks);
        }

        [HttpGet("getAllTaskByProjectID/{projectId}")]
        public IActionResult GetTasksByProjectId(int projectId)
        {
            var taskDtos = dbContext.Tasks
                .Where(t => t.ProjectId == projectId)
                .Select(t => new TaskEntity
                {
                    Id = t.Id,
                    Description = t.Description,
                    Name = t.Name,
                    Startdate = t.Startdate,
                    Enddate = t.Enddate,
                    Priority = t.Priority,
                    Progress = t.Progress,
                    ProjectId = t.ProjectId,
                    AssignedTo = t.AssignedTo

                })
                .ToList();

            return Ok(taskDtos);
        }

        [HttpGet("countTasksByProject/{projectId}")]
        public IActionResult CountTasksByProjectId(int projectId)
        {
            bool projectExists = dbContext.Tasks.Any(p => p.ProjectId == projectId);
            if (!projectExists)
            {
                return NotFound(new { Message = $"Project with ID {projectId} not found." });
            }
            var projectName = dbContext.Projects.Where(t => t.ProjectId == projectId).Select(t => t.Name).FirstOrDefault();
            var totalTasks = dbContext.Tasks.Count(t => t.ProjectId == projectId);
            var completeTasks = dbContext.Tasks.Count(t => t.ProjectId == projectId && t.Status == "Completed");
            var inProgressTasks = dbContext.Tasks.Count(t => t.ProjectId == projectId && t.Status == "In Progress");
            var incompleteTasks = dbContext.Tasks.Count(t => t.ProjectId == projectId && t.Status == "Not Started");

            var tasksByStatus = dbContext.Tasks.Where(t => t.ProjectId == projectId).GroupBy(t => t.Status).ToDictionary(g => g.Key,g => g.Select(task => new {
               task.Id,
               task.Description,
               task.Startdate,
               task.Enddate,
               task.Progress
           }).ToList()
       );

            return Ok(new
            {
                TotalTasks = totalTasks,
                CompleteTasks = completeTasks,
                InProgressTasks = inProgressTasks,
                IncompleteTasks = incompleteTasks,
                ProjectName = projectName,
                TaskbyStatus = tasksByStatus
            });
        }

        [HttpPost("createTask")]
        public async Task<IActionResult> CreateTask(AddTaskDTO addTaskDTO)
        {
            var project = await dbContext.Projects.FirstOrDefaultAsync(p => p.ProjectId == addTaskDTO.ProjectId);
            if (project == null)
                return NotFound($"Project with ID {addTaskDTO.ProjectId} not found.");

            var taskEntity = new TaskEntity
            {
                Name = addTaskDTO.TaskName,
                Description = addTaskDTO.Description,
                Startdate = addTaskDTO.Startdate,
                Enddate = addTaskDTO.Enddate,
                Priority = addTaskDTO.Priority,
                ProjectId = addTaskDTO.ProjectId,
                AssignedTo = addTaskDTO.AssignedTo,
                Progress = 0.0,
                Status = "Not Started" // default until milestones are added
            };

            dbContext.Tasks.Add(taskEntity);
            await dbContext.SaveChangesAsync();
            await _progressService.UpdateProjectProgress(addTaskDTO.ProjectId);
            
            return Ok(new
            {
                Message = "Task created successfully and project progress updated.",
                Task = taskEntity
            });
        }


        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult DeleteTask(Guid id)
        {
            var task = dbContext.Tasks.FirstOrDefault(db => db.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            dbContext.Tasks.Remove(task);
            dbContext.SaveChanges();
            return Ok("Project deleted");
        }

        [HttpGet("getAllUsers")]
        public IActionResult GetAllUsers()
        {
            var AllUsers = dbContext.Users.ToList();

            return Ok(AllUsers);
        }

        [HttpGet("getAllForemen")]
        public IActionResult GetAllForemen()
        {
            
            var allUsers = dbContext.Users.ToList();
            Console.WriteLine($"Total users in database: {allUsers.Count}");
            foreach (var user in allUsers)
            {
                Console.WriteLine($"User: Id={user.UserId}, Name={user.Name}, Role={user.UserRole}");
            }

            var foremen = dbContext.Users
                          .Where(u => u.UserRole == "Foremen")
                          .Select(u => new
                          {
                              Id = u.UserId,
                              Name = u.Name,
                              UserRole = u.UserRole
                          })
                          .ToList();

            Console.WriteLine($"Found {foremen.Count} foremen:");
            foreach (var foreman in foremen)
            {
                Console.WriteLine($"Foreman: Id={foreman.Id}, Name={foreman.Name}");
            }

            return Ok(foremen);
        }





    }
}
