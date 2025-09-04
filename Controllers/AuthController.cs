using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using ProBuild_Api.Models;
using ProBuild_API.Data;
using ProBuild_API.DTOs;

[Route("api/webauth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ProBuildDbContext _context;

    public AuthController(ProBuildDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
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

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                name = user.Name,
                surname = user.Surname,
                email = user.Email,
                address = user.Address,
                contact = user.Contact,
                role = user.UserRole
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Registration failed: {ex.Message}" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return Unauthorized(new { error = "Invalid email or password" });
            }

            var response = new LoginResponseDto
            {
                UserId = user.UserId,
                UserRole = user.UserRole ,
                UserName = user.Name

            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Login failed: {ex.Message}" });
        }
    }

    [HttpGet("test-db")]
    public IActionResult TestDatabaseConnection()
    {
        try
        {
            _context.Database.OpenConnection();
            _context.Database.CloseConnection();
            return Ok("Database connection successful");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database connection failed: {ex.Message}");
        }
    }
}