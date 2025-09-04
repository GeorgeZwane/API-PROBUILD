// File: ProBuild_API.Controllers.MilestoneController.cs
// This is the backend API controller that handles data operations.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data;
using ProBuild_API.DTOs;
using ProBuild_API.Models;
using ProBuildWebAPI_v2_.Models;

namespace ProBuild_API.Controllers
{
    [ApiController] // Use ApiController attribute
    [Route("api/webmilestone")] // General route prefix
    public class MilestoneController : Controller
    {
        private readonly ProBuildDbContext dbContext;
        public MilestoneController(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

  [HttpPost("{taskId}/addMilestone")]
        public IActionResult AddMilestone(Guid taskId, [FromBody] MileStoneDTO dto)
        {
            var taskExists = dbContext.Tasks.Any(t => t.Id == taskId);
            if (!taskExists)
                return NotFound($"Task with ID {taskId} not found.");

            // Create and add milestone
            var milestone = new Milestone
            {
                MilestoneName = dto.MilestoneName,
                Description = dto.Description,
                Reason = dto.Reason,
                Status = "InComplete",
                DueDate = dto.DueDate,
                TaskEntityId = taskId
            };

            dbContext.Milestones.Add(milestone);
            dbContext.SaveChanges();

            return Ok(new
            {
                Message = "Milestone added successfully.",
                Milestone = milestone
            });
        }

        [HttpGet("{taskId}/GetMilestonesByTaskID")]
        public IActionResult GetMilestonesByTaskId(Guid taskId)
        {
            var taskMilestones = dbContext.Milestones
                .Where(t => t.TaskEntityId == taskId).ToList();

            if (taskMilestones == null)
                return NotFound("taskMilestones not found");

            return Ok(taskMilestones);
        }




     // NEW ACTION: Get a single milestone by its ID
        [HttpGet("GetMilestoneById/{milestoneId}")]
        public IActionResult GetMilestoneById(Guid milestoneId)
        {
            var milestone = dbContext.Milestones.FirstOrDefault();

            if (milestone == null)
            {
                return NotFound($"Milestones not found.");
            }

            return Ok(milestone);
        }

        [HttpPut("{MilestoneId}/updateMilestone")]
        public IActionResult UpdateTask(Guid MilestoneId, [FromBody] MileStoneDTO dto)
        {
            if (dto == null || MilestoneId != dto.Id)
                return BadRequest("Invalid task data.");

            var existingTask = dbContext.Milestones.FirstOrDefault(t => t.Id == MilestoneId);
            if (existingTask == null)
                return NotFound($"Milestone with ID {MilestoneId} not found.");

            existingTask.Reason = dto.Reason;
            existingTask.Status = dto.Status;

            try
            {
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the task.");
            }

            return Ok("Milestone Updated Successfully");
        }
    }
}
