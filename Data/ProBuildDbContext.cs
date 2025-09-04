using Microsoft.EntityFrameworkCore;
using ProBuild_Api.Models; 
using ProBuild_API.Models;
using ProBuildWebAPI_v2_.Models;
using Project = ProBuildWebAPI_v2_.Models.Project; 


namespace ProBuild_API.Data
{
    public class ProBuildDbContext : DbContext
    {
        public ProBuildDbContext(DbContextOptions<ProBuildDbContext> options)
            : base(options)
        {
        }

 
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; } 
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Material> Materials { get; set; } 

    
        public DbSet<UserTaskAssignment> UserTaskAssignments { get; set; } 
        public DbSet<ProjectTeam> ProjectTeams { get; set; }
        public DbSet<Milestone> Milestones { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>()
    .HasOne(n => n.Sender)
    .WithMany()
    .HasForeignKey(n => n.SenderId)
    .OnDelete(DeleteBehavior.Restrict); // Fixes the cycle issue

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Recipient)
                .WithMany()
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.Restrict); // Also safe

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email) 
                .IsUnique();

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.ProjectId);

                entity.HasOne(p => p.User)         
                      .WithMany()                  
                      .HasForeignKey(p => p.UserId) 
                      .IsRequired()             
                      .OnDelete(DeleteBehavior.Restrict); 

                entity.Property(p => p.Startdate).HasColumnType("datetime2");
                entity.Property(p => p.Enddate).HasColumnType("datetime2");
            });

           
            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Project)       
                      .WithMany(p => p.Equipment)  
                      .HasForeignKey(e => e.ProjectId) 
                      .IsRequired()                 
                      .OnDelete(DeleteBehavior.Cascade); 

            });

            modelBuilder.Entity<Material>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasOne(m => m.Project)
                      .WithMany() 
                      .HasForeignKey(m => m.ProjectId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<TaskEntity>(entity =>
            {
                entity.HasKey(t => t.Id);

                //entity.HasOne(t => t.Project)         
                //      .WithMany(p => p.Tasks)       
                //      .HasForeignKey(t => t.ProjectId) 
                //      .IsRequired()                
                //      .OnDelete(DeleteBehavior.Cascade); 

                entity.Property(t => t.Startdate).HasColumnType("datetime2");
                entity.Property(t => t.Enddate).HasColumnType("datetime2");

                entity.Property(t => t.Progress).HasColumnType("float"); 
            });

            // Call base method to apply any default conventions
            base.OnModelCreating(modelBuilder);
        }
    }
}
