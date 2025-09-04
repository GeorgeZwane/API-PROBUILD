using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProBuild_Api.Models; 
using ProBuild_API.Data; 
using ProBuild_API.DTOs;
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims;
using System.Text; 
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

[Route("api/auth")]
[ApiController]
public class AppAuthController : ControllerBase
{
    private readonly ProBuildDbContext _context;
    private readonly IConfiguration _configuration; // Inject IConfiguration

    public AppAuthController(ProBuildDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration; // Initialize IConfiguration
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AppUserRegisterDto dto)
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
                Contact = dto.Contact
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // After successful registration, generate a token for the new user
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                name = user.Name,
                surname = user.Surname,
                email = user.Email,
                address = user.Address,
                contact = user.Contact,
                role = user.UserRole,
                token = token // Return the generated token
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Registration failed: {ex.Message}" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AppUserLoginDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                return Unauthorized(new { error = "Invalid email or password" });
            }

            // Generate JWT token for the authenticated user
            var token = GenerateJwtToken(user);

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Login failed: {ex.Message}" });
        }
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
