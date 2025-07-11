using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace practice_quiz.Models
{
    [Table("quiz")]
    public class Quiz
    {
        [Key]
        [Column("quiz_id")]
        public int QuizId { get; set; }

        [Column("category")]
        public string Category { get; set; } = null!;

        [Column("description")]
        public string Description { get; set; } = null!;

        [Column("option_1")]
        public string? Option1 { get; set; }

        [Column("option_2")]
        public string? Option2 { get; set; }

        [Column("option_3")]
        public string? Option3 { get; set; }

        [Column("option_4")]
        public string? Option4 { get; set; }

        [Column("answer_num")]
        public int Answer { get; set; }
    }
}
