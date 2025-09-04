namespace ProBuild_API.DTOs
{
    public class ProjectReportDTO
    {
        public string? ProjectName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int TasksInProgress { get; set; }
        public double Budget { get; set; }
    }
}
