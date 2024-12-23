using Microsoft.EntityFrameworkCore;

namespace ChecklistManager.Models
{
    public class ChecklistTaskContext : DbContext
    {
        public ChecklistTaskContext(DbContextOptions<ChecklistTaskContext> options) : base(options) { }

        public DbSet<ChecklistTask> Tasks { get; set; } = null!;
    }
}
