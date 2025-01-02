using Microsoft.EntityFrameworkCore;

namespace ChecklistManager.Models
{
    public class MemberContext : DbContext
    {
        public MemberContext(DbContextOptions<MemberContext> options) : base(options) { }

        public DbSet<Member> Members { get; set; } = null!;
    }
}
