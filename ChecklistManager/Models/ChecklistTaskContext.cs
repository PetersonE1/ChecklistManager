using Microsoft.EntityFrameworkCore;

namespace ChecklistManager.Models
{
    public class ChecklistTaskContext : DbContext
    {
        public ChecklistTaskContext(DbContextOptions<ChecklistTaskContext> options) : base(options) { }

        public DbSet<ChecklistTask> Tasks { get; set; } = null!;

        public async Task DailyReset()
        {
            foreach (var task in Tasks)
            {
                if (task.State == TaskState.Complete)
                {
                    if (task.Schedule != null)
                    {
                        task.State = TaskState.Incomplete;
                        task.IsActive = false;
                    }
                    else
                    {
                        Tasks.Remove(task);
                    }
                }
                
                if (task.Schedule?.IsSatisfiedBy(DateTime.Now) ?? true)
                {
                    task.IsActive = true;
                }
            }
            await SaveChangesAsync();
        }
    }
}
