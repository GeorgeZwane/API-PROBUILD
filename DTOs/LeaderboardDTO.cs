namespace ProBuild_API.DTOs
{
    public class LeaderboardDTO
    {
      
            public int UserId { get; set; }
            public string FullName { get; set; } // or just Name
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int IncompletedTasks { get; set; }

        public int TotalMilestones { get; set; }
        public int CompletedMilestone { get; set; }
        public int IncompletedMilestone { get; set; }

        public double AverageTaskCompletionRate { get; set; }
        public double AverageMilestoneCompletionRate { get; set; }

    }
}
