namespace ProBuild_API.DTOs
{
    public class ProjectResponseDTO
    {
        public  int projectId { get; set; }
        public required string Name { get; set; }
        public required string Location { get; set; }
        public required string Startdate { get; set; }
        public required string Enddate { get; set; }
        public  double? Progress { get; set; }
        public string? Description { get; set; }
        public List<TaskResponseDTO>? Tasks { get; set; }
    }

}
