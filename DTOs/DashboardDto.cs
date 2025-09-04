using ProBuild_API.Models;

namespace ProBuild_API.DTOs
{
    public class DashboardDto
    {
       
        public object RecentActivities { get; set; }
        public object RevenueLabels { get; set; }
        public object RevenueValues { get; set; }
    }
}