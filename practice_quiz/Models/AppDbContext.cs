using Microsoft.EntityFrameworkCore;

namespace practice_quiz.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public virtual DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }
    }
}
