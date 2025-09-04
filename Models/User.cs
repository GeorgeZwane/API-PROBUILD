using System.ComponentModel.DataAnnotations;

namespace ProBuild_Api.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; } 

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [EmailAddress] // Provides email validation
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty; 

        [Required]
        public string Address { get; set; } = string.Empty;


        public string? UserRole { get; set; }

        [Required]
        [Phone] // Provides phone number validation
        public string Contact { get; set; } 

        // public ICollection<Project> Projects { get; set; } = new List<Project>();
        // public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
    }
}
