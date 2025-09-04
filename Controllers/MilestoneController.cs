using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data;
using ProBuild_API.DTOs;
using ProBuild_API.Models;
using ProBuildWebAPI_v2_.Models;

namespace ProBuild_API.Controllers
{
    [ApiController]
    [Route("api/webmilestone")]
    public class MilestoneController : ControllerBase
    {
        private readonly ProBuildDbContext dbContext;

        public MilestoneController(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("{taskId}/addMilestone")]
        public async Task<IActionResult> AddMilestone(Guid taskId, [FromBody] MileStoneDTO dto)
        {
            var taskExists = await dbContext.Tasks.AnyAsync(t => t.Id == taskId);
            if (!taskExists)
                return NotFound($"Task with ID {taskId} not found.");

            var milestone = new Milestone
            {
                MilestoneName = dto.MilestoneName,
                Description = dto.Description,
                Reason = dto.Reason,
                Status = "Not Started",
                DueDate = dto.DueDate,
                TaskEntityId = taskId
            };

            await dbContext.Milestones.AddAsync(milestone);
            await dbContext.SaveChangesAsync();

            await UpdateTaskProgress(taskId);

            return Ok(new
            {
                Message = "Milestone added successfully and task progress updated.",
                Milestone = milestone
            });
        }

        [HttpPut("{MilestoneId}/updateMilestone")]
        public async Task<IActionResult> UpdateMilestone(Guid MilestoneId, [FromBody] UpdateMilestoneDTO dto)
        {
            if (dto == null || MilestoneId != dto.Id)
                return BadRequest("Invalid milestone data. ID mismatch.");

            var existingMilestone = await dbContext.Milestones.FindAsync(MilestoneId);
            if (existingMilestone == null)
                return NotFound($"Milestone with ID {MilestoneId} not found.");

            existingMilestone.Reason = dto.Reason;
            existingMilestone.Status = dto.Status;

            try
            {
                await dbContext.SaveChangesAsync();
                await UpdateTaskProgress(existingMilestone.TaskEntityId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating milestone: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the milestone.");
            }

            return Ok("Milestone Updated and Task Progress Recalculated Successfully");
        }

        [HttpGet("{taskId}/GetMilestonesByTaskID")]
        public async Task<IActionResult> GetMilestonesByTaskId(Guid taskId)
        {
            var taskMilestones = await dbContext.Milestones
                .Where(t => t.TaskEntityId == taskId)
                .ToListAsync();

            if (taskMilestones == null || !taskMilestones.Any())
                return NotFound("No milestones found for this task.");

            return Ok(taskMilestones);
        }

        [HttpGet("{milestoneId}")]
        public async Task<IActionResult> GetMilestoneById(Guid milestoneId)
        {
            var milestone = await dbContext.Milestones.FirstOrDefaultAsync(m => m.Id == milestoneId);
            if (milestone == null)
                return NotFound($"Milestone with ID {milestoneId} not found.");

            return Ok(milestone);
        }

        private async Task UpdateTaskProgress(Guid taskId)
        {
            var task = await dbContext.Tasks.FindAsync(taskId);
            if (task == null) return;

            var milestones = await dbContext.Milestones
                .Where(m => m.TaskEntityId == taskId)
                .ToListAsync();

            if (milestones.Count == 0)
            {
                task.Progress = 0;
                task.Status = "Not Started";
                await dbContext.SaveChangesAsync();
                await UpdateProjectProgress(task.ProjectId);
                return;
            }

            var completedCount = milestones.Count(m => m.Status == "Completed");
            var progress = (double)completedCount / milestones.Count * 100;

            task.Progress = Math.Round(progress, 2);

            task.Status = progress == 0 ? "Not Started"
                         : progress < 100 ? "InProgress"
                         : "Completed";

            await dbContext.SaveChangesAsync();
            await UpdateProjectProgress(task.ProjectId);
        }

        private async Task UpdateProjectProgress(int projectId)
        {
            var project = await dbContext.Projects.FindAsync(projectId);
            if (project == null) return;

            var tasks = await dbContext.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            if (tasks.Count == 0)
            {
                project.Progress = 0;
                project.Status = "Not Started";
                await dbContext.SaveChangesAsync();
                return;
            }

            var totalProgress = tasks.Sum(t => t.Progress);
            var averageProgress = totalProgress / tasks.Count;

            project.Progress = (double?)Math.Round((decimal)averageProgress, 2);

            project.Status = averageProgress == 0 ? "Not Started"
                             : averageProgress < 100 ? "In Progress"
                             : "Completed";

            await dbContext.SaveChangesAsync();
        }
    }
}
