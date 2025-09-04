using Microsoft.EntityFrameworkCore;
using ProBuild_API.Data;


namespace ProBuild_API.Service
{
  

    public class ProgressService
    {
        private readonly ProBuildDbContext dbContext;

        public ProgressService(ProBuildDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task UpdateProjectProgress(int projectId)
        {
            var project = await dbContext.Projects.FindAsync(projectId);
            if (project == null) return;

            var tasks = await dbContext.Tasks
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            if (tasks.Count == 0)
            {
                project.Progress = 0;
                project.Status = "InWaiting";
            }
            else
            {
                var totalProgress = tasks.Sum(t => t.Progress);
                var averageProgress = totalProgress / tasks.Count;

                project.Progress = (double?)Math.Round((decimal)averageProgress, 2);
                project.Status = averageProgress == 0 ? "InWaiting"
                                 : averageProgress < 100 ? "In Progress"
                                 : "Complete";
            }

            await dbContext.SaveChangesAsync();
        } 
    }

}
