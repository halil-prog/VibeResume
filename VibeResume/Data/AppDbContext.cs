using Microsoft.EntityFrameworkCore;
using VibeResume.Models;

namespace VibeResume.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ResumeHistory> ResumeHistories { get; set; }
    }
}