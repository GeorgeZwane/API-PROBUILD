namespace ProBuild_API.DTOs
{
    public class UserLoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? Role { get; set; }
    }
}
