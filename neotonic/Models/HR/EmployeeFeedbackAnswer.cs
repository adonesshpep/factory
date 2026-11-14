using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models.HR
{
    [Table("employee_feedback_answers")]
    [Index(nameof(EmployeeFeedbackId), nameof(QuestionNumber), IsUnique = true)]
    public class EmployeeFeedbackAnswer
    {
        [Key]
        public int Id { get; set; }

        [Column("employee_feedback_id")]
        public int EmployeeFeedbackId { get; set; }
        public EmployeeFeedback EmployeeFeedback { get; set; } = null!;

        // 1..10, the question texts live in FeedbackQuestions (not in the DB)
        [Column("question_number")]
        public int QuestionNumber { get; set; }

        [Column("score")]
        public FeedbackScore Score { get; set; }

        [MaxLength(1000)]
        [Column("reason")]
        public string? Reason { get; set; }
    }
}