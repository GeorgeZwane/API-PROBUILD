using Microsoft.AspNetCore.Mvc;
using ProBuild_API.Data;
using ProBuildWebAPI_v2_.DTOs;

namespace ProBuild_API.Controllers
{
    [Route("api/Admin")]
    [ApiController]
    public class AdminController : Controller
    {

        private readonly ProBuildDbContext dbContext;
        public AdminController(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("getAllUsers")]
        public IActionResult GetAllUsers()
        {
            var allUserss = dbContext.Users
                .Select(p => new AddUserDTO
                {
                    Name = p.Name,
                    Surname = p.Surname,
                    Email = p.Email,
                    Password = p.Password,
                    Address = p.Address,
                    Contact = p.Contact,
                    UserRole = p.UserRole
                })
                .ToList();

            return Ok(allUserss);
        }

        [HttpPost("assignRole")]
        public IActionResult AssignUserRole([FromBody] AddUserDTO model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.UserRole))
            {
                return BadRequest("Email and Role are required.");
            }

            var user = dbContext.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.UserRole = model.UserRole;
            dbContext.SaveChanges();

            return Ok($"Role '{model.UserRole}' assigned to user '{user.Email}'.");
        }

    }
}
