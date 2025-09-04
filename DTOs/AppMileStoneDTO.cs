namespace ProBuild_API.DTOs
{
    public class AppMileStoneDTO
    {
        public Guid Id { get; set; }
        public required string Goal { get; set; }
        public required Boolean CompletedOrNot { get; set; }
    }
}
