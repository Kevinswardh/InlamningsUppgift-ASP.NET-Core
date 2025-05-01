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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🎯 Budget precision
            modelBuilder.Entity<ProjectEntity>()
                .Property(p => p.Budget)
                .HasPrecision(18, 2);

            // 📌 Relation: Project ↔ Customer
            modelBuilder.Entity<ProjectEntity>()
                .HasOne(p => p.Customer)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 📌 Relation: ProjectMember (many-to-many via mellan-tabell)
            modelBuilder.Entity<ProjectMemberEntity>()
                .HasKey(pm => new { pm.ProjectId, pm.TeamMemberId });

            modelBuilder.Entity<ProjectMemberEntity>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.ProjectMembers)
                .HasForeignKey(pm => pm.ProjectId);

            modelBuilder.Entity<ProjectMemberEntity>()
                .HasOne(pm => pm.TeamMember)
                .WithMany()
                .HasForeignKey(pm => pm.TeamMemberId);
        }
    }
}
