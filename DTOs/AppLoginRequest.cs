namespace ProBuild_API.DTOs
{
    public class AppLoginRequest
    {
        public required string Email { get; set; }

        public required string Password { get; set; }
    }
}
