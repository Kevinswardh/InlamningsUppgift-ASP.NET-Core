using DomainLayer_BusinessLogicLayer_.DomainModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5.DataAccessLayer_DAL_
{
    public class ApplicationChatDbContext : DbContext
    {
        public DbSet<ChatMessageEntity> ChatMessages { get; set; }

        public ApplicationChatDbContext(DbContextOptions<ApplicationChatDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatMessageEntity>()
                .HasKey(c => c.Id);
        }
    }

}
