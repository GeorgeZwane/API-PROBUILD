namespace ProBuildWebAPI_v2_.DTOs
{
    public class AppAddUserDTO
    {
        public required string Name { get; set; }

        public required string Surname { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public required string Address { get; set; }

        public required string Contact { get; set; }

        public string? UserRole { get; set; }
    }
}
