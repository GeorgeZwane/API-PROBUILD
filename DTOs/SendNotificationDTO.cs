namespace ProBuild_API.DTOs
{
    public class SendNotificationDTO
    {
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public string? Message { get; set; }
    }
}
