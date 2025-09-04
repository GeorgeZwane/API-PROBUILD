namespace ProBuildWebAPI_v2_.DTOs
{
    public class AddProjectDTO
    {
        public int ProjectId { get; set; }
        public required string Name { get; set; }

        public required string Location { get; set; }

        public required DateTime Startdate { get; set; }

        public required DateTime Enddate { get; set; }
        public string? Status { get; set; }
        public double? Progress { get; set; }
        public required double Budget { get; set; }

        public string? Description { get; set; }
        //public required List<AddTaskDTO> TaskEntities { get; set; }
        public int UserId { get; set; }
    }
   
    }
