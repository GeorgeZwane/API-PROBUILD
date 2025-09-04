using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProBuild_API.Data;
using ProBuild_API.DTOs;
using ProBuild_API.Models;
using ProBuildWebAPI_v2_.DTOs;
using ProBuildWebAPI_v2_.Models;
using Task = ProBuildWebAPI_v2_.Models;


namespace ProBuildWebAPI_v2_.Controllers
{
    [Route("api/webtask")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ProBuildDbContext dbContext;
        public TaskController(ProBuildDbContext dbContext)
        {
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

        [HttpGet("getAllTaskByUserID/{userId}")]
        public IActionResult GetTasksByUserId(int userId)
        {
            var taskDtos = dbContext.Tasks
                .Where(t => t.UserId == userId)
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

        [HttpGet("getLeaderboard")]
        public IActionResult GetForemanLeaderboard()
        {
            var leaderboard = dbContext.Users
      .Where(u => u.UserRole == "Foreman")
      .Select(u => new LeaderboardDTO
      {
          UserId = u.UserId,
          FullName = u.Name,

          // Tasks
          TotalTasks = dbContext.Tasks.Count(t => t.UserId == u.UserId),
          CompletedTasks = dbContext.Tasks.Count(t => t.UserId == u.UserId && t.Progress == 100),
          IncompletedTasks = dbContext.Tasks.Count(t => t.UserId == u.UserId && t.Progress < 100),

          // Milestones
        //  TotalMilestones = dbContext.Milestones.Count(m => m.UserId == u.UserId),
        //  CompletedMilestone = dbContext.Milestones.Count(m => m.UserId == u.UserId && m.Status == "Completed"),
        //  IncompletedMilestone = dbContext.Milestones.Count(m => m.UserId == u.UserId && m.Status != "Completed")
      })
      .AsEnumerable() // Do math client-side for better precision
      .Select(dto =>
      {
          dto.AverageTaskCompletionRate = dto.TotalTasks > 0
              ? (double)dto.CompletedTasks / dto.TotalTasks * 100
              : 0;

       //   dto.AverageMilestoneCompletionRate = dto.TotalMilestones > 0
        //      ? (double)dto.CompletedMilestone / dto.TotalMilestones * 100
          //    : 0;

          return dto;
      })
      .OrderByDescending(dto => dto.CompletedTasks)
      .ToList();

            return Ok(leaderboard);
        }

        [HttpGet("updateProject/{projectId}")]
        public IActionResult UpdateProject(int projectId)
        {
            // Get all tasks for the project
            var tasks = dbContext.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToList();

            if (!tasks.Any())
            {
                return NotFound($"No tasks found for project ID {projectId}");
            }

            // Determine project status based on tasks
            string newProjectStatus;

            if (tasks.All(t => t.Status == "Complete"))
            {
                newProjectStatus = "Complete";
            }
            else if (tasks.Any(t => t.Status == "In Progress"))
            {
                newProjectStatus = "In Progress";
            }
            else if (tasks.Any(t => t.Status == "Incomplete"))
            {
                newProjectStatus = "Incomplete";
            }
            else
            {
                // Fallback status (if tasks have some other status)
                newProjectStatus = "In Progress";
            }

            // Update project status if changed
            var project = dbContext.Projects.FirstOrDefault(p => p.ProjectId == projectId);
            if (project != null && project.Status != newProjectStatus)
            {
                project.Status = newProjectStatus;
                project.Progress = tasks.Average(t => t.Progress);
                dbContext.SaveChanges();
            }

            // Map tasks to DTO
            var taskDtos = tasks.Select(t => new TaskEntity
            {
                Id = t.Id,
                Description = t.Description,
                Name = t.Name,
                Startdate = t.Startdate,
                Enddate = t.Enddate,
                Priority = t.Priority,
                Progress = t.Progress,
                ProjectId = t.ProjectId,
                AssignedTo = t.AssignedTo,
                Status = t.Status  // You might want to include Status too
            }).ToList();

            return Ok(taskDtos);
        }


        [HttpGet("updateTaskUsingMilestones/{projectId}")]
        public IActionResult UpdateTaskTEST(int projectId)
        {
            var tasks = dbContext.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToList();

            if (!tasks.Any())
            {
                return NotFound($"No tasks found for project ID {projectId}");
            }

            foreach (var task in tasks)
            {
                var milestones = dbContext.Milestones
                    .Where(m => m.TaskEntityId == task.Id)
                    .ToList();

                if (!milestones.Any())
                {
                    task.Status = "Incomplete";
                    task.Progress = 0;
                    continue;
                }

                int total = milestones.Count;
                int completed = milestones.Count(m => m.Status == "Complete");

                if (completed == total)
                {
                    task.Status = "Complete";
                }
                else if (completed > 0)
                {
                    task.Status = "In Progress";
                }
                else
                {
                    task.Status = "Incomplete";
                }

                double progress = ((double)completed / total) * 100;
                task.Progress = Math.Round(progress, 2);

                // ✅ Print progress to console
                Console.WriteLine($"Task has milestones complete: {completed}");

                Console.WriteLine($"Task: {task.Name} | Progress: {task.Progress}% | Status: {task.Status}");



            }

            dbContext.SaveChanges();

            // Optionally return the updated tasks as DTOs
            var taskDtos = tasks.Select(t => new
            {
                t.Name,
                t.Status,
                t.Progress,

            }).ToList();

            return Ok(taskDtos);
        }



        [HttpGet("getAllInCompleteTaskByProjectID/{projectId}")]
        public IActionResult GetIncompleteTasksByProjectId(int projectId)
        {
            var taskDtos = dbContext.Tasks
                .Where(t => t.ProjectId == projectId && t.Status == "Incomplete")
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


        [HttpGet("getAllProgressTaskByProjectID/{projectId}")]
        public IActionResult GetProgressTasksByProjectId(int projectId)
        {
            var taskDtos = dbContext.Tasks
                .Where(t => t.ProjectId == projectId && t.Status == "In Progress")
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


        [HttpGet("getAllCompletedTaskByProjectID/{projectId}")]
        public IActionResult GetCompletedTasksByProjectId(int projectId)
        {
            var taskDtos = dbContext.Tasks
                .Where(t => t.ProjectId == projectId && t.Status =="Complete")
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
            var completeTasks = dbContext.Tasks.Count(t => t.ProjectId == projectId && t.Status == "Complete");
            var inProgressTasks = dbContext.Tasks.Count(t => t.ProjectId == projectId && t.Status == "In Progress");
            var incompleteTasks = dbContext.Tasks.Count(t => t.ProjectId == projectId && t.Status == "Incomplete");

            //var tasksName = dbContext.Tasks.Count(t => t.ProjectId == projectId && t.Status == "Incomplete");

            //var taskStartDate = dbContext.Tasks.Where(t => t.ProjectId == projectId).Select(t => t.Startdate).ToList();
            //var taskEndDate = dbContext.Tasks.Where(t => t.ProjectId == projectId).Select(t => t.Enddate).ToList();


          
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

                //TaskName = tasksName,
                //StartDate = taskStartDate,
                //EndDate = taskEndDate,

                TaskbyStatus = tasksByStatus
            });
        }

        [HttpPost("createTask")]
        public IActionResult CreateTask(AddTaskDTO addTaskDTO)
        {
            var project = dbContext.Projects.FirstOrDefault(p => p.ProjectId == addTaskDTO.ProjectId);
            if (project == null)
            {
                return NotFound($"Project with ID {addTaskDTO.ProjectId} not found.");
            }

            var taskEntity = new TaskEntity()
            {
                Name = addTaskDTO.TaskName,
                Description = addTaskDTO.Description,
                Startdate = addTaskDTO.Startdate,
                Enddate = addTaskDTO.Enddate,
                Progress = 0.0,
                Status = "In Progress",
                Priority =addTaskDTO.Priority,
                ProjectId = addTaskDTO.ProjectId,
                UserId =addTaskDTO.UserId,
          
            };
            dbContext.Tasks.Add(taskEntity);
            dbContext.SaveChanges();

            // After saving the task, send notification to assigned user
            var notification = new Notification
            {
                SenderId = null,
                RecipientId = addTaskDTO.UserId,
                Message = $"You have been assigned to the task: {taskEntity.Name}",
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            dbContext.Notifications.Add(notification);
            dbContext.SaveChanges();

            return Ok("Task has been created successfully and notification sent to user assigned");
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
            var foreman = dbContext.Users
                          .Where(u => u.UserRole == "Foreman") // or whatever the role value is
                          .ToList();

            return Ok(foreman);
        }

    }
}
