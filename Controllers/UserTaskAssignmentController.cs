using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data;
using ProBuildWebAPI_v2_.Models;


namespace ProBuildWebAPI_v2_.Controllers
{
    [Route("api/ProBuild")]
    [ApiController]
    public class UserTaskAssignmentController : ControllerBase
    {
        private readonly ProBuildDbContext dbContext;
        public UserTaskAssignmentController(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost]
        [Route("api/assign")]
        public async Task<IActionResult> AssignUserToTask([FromBody] UserTaskAssignmentDTO dto)
        {
            var user = await dbContext.Users.FindAsync(dto.UserId);
            var task = await dbContext.Tasks.FindAsync(dto.TaskEntityId);

            if (user == null || task == null)
                return NotFound("User or Task not found.");

            var assignmentExists = await dbContext.UserTaskAssignments
                .AnyAsync(a => a.UserId == dto.UserId && a.TaskEntityId == dto.TaskEntityId);

            if (assignmentExists)
                return BadRequest("User is already assigned to this task.");

            var assignment = new UserTaskAssignment
            {
                UserId = dto.UserId,
                TaskEntityId = dto.TaskEntityId,
                RoleOnTask = dto.RoleOnTask,
                AssignedDate = DateTime.UtcNow
            };

            dbContext.UserTaskAssignments.Add(assignment);
            await dbContext.SaveChangesAsync();

            return Ok("User assigned to task.");
        }
    }
}
