using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace practice_quiz.Models
{
    [Table("leaderboard")]
    public class Leaderboard
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("score")]
        public int Score { get; set; }
    }
}
