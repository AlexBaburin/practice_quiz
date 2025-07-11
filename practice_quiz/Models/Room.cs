using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace practice_quiz.Models
{
    [Table("room")]
    public class Room
    {
        [Key]
        [Column("room_code")]
        public string RoomCode { get; set; } = null!;

        [Column("host_connection_id")]
        public string HostConnectionId { get; set; } = null!;

        [Column("game_started")]
        public bool GameStarted { get; set; }

        [InverseProperty("Room")]
        public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    }

    public class RoomViewModel
    {
        public string RoomCode { get; set; }
        public bool IsHost { get; set; }
        public List<PlayerViewModel> Players { get; set; } = new List<PlayerViewModel>();
        public bool GameStarted { get; set; }
    }
}
