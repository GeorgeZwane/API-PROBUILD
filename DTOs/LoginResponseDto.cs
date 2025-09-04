namespace ProBuild_API.DTOs
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string? UserRole { get; set; }
    }
}
