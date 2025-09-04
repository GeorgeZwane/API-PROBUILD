using ProBuild_Api.Models;
using System.ComponentModel.DataAnnotations;

namespace ProBuild_API.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public int? SenderId { get; set; }       // Project Manager
        public int RecipientId { get; set; }    // Foreman
        public string? Message { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; } = false;

        // Navigation (optional)
        public User? Sender { get; set; }
        public User? Recipient { get; set; }
    }
}
