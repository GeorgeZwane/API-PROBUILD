namespace ProBuild_API.DTOs
{
    public class AppProjectResponseDTO
    {
        public  int projectId { get; set; }
        public required string Name { get; set; }
        public required string Location { get; set; }
        public required string Startdate { get; set; }
        public required string Enddate { get; set; }
        public  double? Progress { get; set; }
        public string? Description { get; set; }
        public List<AppTaskResponseDTO>? Tasks { get; set; }
    }

}
