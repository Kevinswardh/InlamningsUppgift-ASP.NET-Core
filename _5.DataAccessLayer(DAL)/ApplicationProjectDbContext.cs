using DomainLayer_BusinessLogicLayer_.DomainModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5.DataAccessLayer_DAL_
{
    public class ApplicationProjectDbContext : DbContext
    {
        public ApplicationProjectDbContext(DbContextOptions<ApplicationProjectDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; } // ✅ ENDAST Project!

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Här kan vi lägga till konfigurationer för Project om vi behöver senare
        }
    }
}
