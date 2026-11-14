using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models.HR
{
    [Table("employee_feedbacks")]
    [Index(nameof(FeedbackDate))]
    public class EmployeeFeedback
    {
        [Key]
        public int Id { get; set; }

        [Column("feedback_date")]
        public DateOnly FeedbackDate { get; set; }

        [Column("department_id")]
        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        // Optional: the form allows anonymous answers
        [Column("worker_id")]
        public int? WorkerId { get; set; }
        public Worker? Worker { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("job_title")]
        public string JobTitle { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public List<EmployeeFeedbackAnswer> Answers { get; set; } = new();
    }
}