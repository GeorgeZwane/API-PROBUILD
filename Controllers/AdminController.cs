using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProBuild_Api.Models;
using ProBuild_API.Data;
using ProBuild_API.DTOs;
using ProBuildWebAPI_v2_.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProBuild_API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : Controller
    {

        private readonly ProBuildDbContext dbContext;
        private readonly IConfiguration _configuration; // Inject IConfiguration

        public AdminController(ProBuildDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            _configuration = configuration; // Initialize IConfiguration
        }

        // Helper method to generate JWT token
        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
              //new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key 'Jwt:Key' not configured in appsettings.json.")
            ));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Token valid for 7 hours
            var expires = DateTime.Now.AddHours(7);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer 'Jwt:Issuer' not configured in appsettings.json."),
                audience: _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience 'Jwt:Audience' not configured in appsettings.json."),
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost("register")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] AppUserRegisterDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await dbContext.Users.AnyAsync(u => u.Email == dto.Email))
                {
                    return BadRequest(new { error = "Email already exists" });
                }

                var user = new User
                {
                    Name = dto.Name,
                    Surname = dto.Surname,
                    Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Email = dto.Email,
                    Address = dto.Address,
                    Contact = dto.Contact,
                    UserRole = dto.UserRole
                };

                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                // After successful registration, generate a token for the new user
                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    name = user.Name,
                    surname = user.Surname,
                    email = user.Email,
                    address = user.Address,
                    contact = user.Contact,
                    userRole = user.UserRole,
                    token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Registration failed: {ex.Message}" });
            }
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
