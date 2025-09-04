namespace ProBuild_API.DTOs
{
    public class AppUserRegisterDto
    {
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }
        public required string Contact { get; set; }
        public required string UserRole { get; set; }
    }
}
