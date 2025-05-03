using DomainLayer_BusinessLogicLayer_.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace _5.DataAccessLayer_DAL_
{
    public class ApplicationProjectDbContext : DbContext
    {
        public ApplicationProjectDbContext(DbContextOptions<ApplicationProjectDbContext> options)
            : base(options)
        {
        }

        // ✅ DbSets
        public DbSet<ProjectEntity> Projects { get; set; }
        public DbSet<CustomerEntity> Customers { get; set; }
        public DbSet<TeamMemberEntity> TeamMembers { get; set; }
        public DbSet<ProjectMemberEntity> ProjectMembers { get; set; }

        // ✅ Ny DbSet för notifikationer
        public DbSet<NotificationEntity> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🎯 Budget precision
            modelBuilder.Entity<ProjectEntity>()
                .Property(p => p.Budget)
                .HasPrecision(18, 2);

            // 📌 TeamMember.Id ska inte autogenereras (det är en string)
            modelBuilder.Entity<TeamMemberEntity>()
                .Property(t => t.Id)
                .ValueGeneratedNever();

            // 📌 Project ↔ Customer (many-to-one)
            modelBuilder.Entity<ProjectEntity>()
                .HasOne(p => p.Customer)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 📌 Project ↔ ProjectMember ↔ TeamMember (many-to-many)
            modelBuilder.Entity<ProjectMemberEntity>()
                .HasKey(pm => new { pm.ProjectId, pm.TeamMemberId });

            modelBuilder.Entity<ProjectMemberEntity>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.ProjectMembers)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectMemberEntity>()
                .HasOne(pm => pm.TeamMember)
                .WithMany(tm => tm.ProjectMembers)
                .HasForeignKey(pm => pm.TeamMemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // 📌 Optional: konfigurera NotificationEntity
            modelBuilder.Entity<NotificationEntity>()
                .Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<NotificationEntity>()
                .Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(500);

            modelBuilder.Entity<NotificationEntity>()
                .Property(n => n.ReceiverUserId)
                .IsRequired();
        }
    }
}
