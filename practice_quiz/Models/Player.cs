using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace practice_quiz.Models
{
    [Table("player")]
    public class Player
    {
        [Key]
        [Column("player_id")]
        public int PlayerId { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("score")]
        public int Score { get; set; }

        [Column("connection_id")]
        public string ConnectionId { get; set; } = null!;

        [Column("room_code")]
        public string RoomCode { get; set; } = null!;
        [ForeignKey("RoomCode")]
        [InverseProperty("Players")]
        public virtual Room Room { get; set; } = null!;
    }

    public class PlayerViewModel
    {
        public int PlayerId { get; set; }
        public string Name { get; set; } = null!;
        public int Score { get; set; }
        public string RoomCode { get; set; } = null!;
    }
}
