using neotonic.Models.HR;
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.HR
{
    public class FeedbackAnswerDto
    {
        [Range(1, 10)]
        public int QuestionNumber { get; set; }

        // 2, 4, 6, 8 or 10
        public FeedbackScore Score { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    // Used for both create and update (the form is filled in one go)
    public class SaveEmployeeFeedbackDto
    {
        // Defaults to today when not sent
        public DateOnly? FeedbackDate { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        // Optional: leave null for an anonymous answer
        public int? WorkerId { get; set; }

        [Required, MaxLength(150)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public List<FeedbackAnswerDto> Answers { get; set; } = new();
    }

    public class FeedbackAnswerResponseDto
    {
        public int QuestionNumber { get; set; }
        public string QuestionTextAr { get; set; } = string.Empty;
        public string QuestionTextEn { get; set; } = string.Empty;
        public int Score { get; set; }
        public string ScoreNameAr { get; set; } = string.Empty;
        public string ScoreNameEn { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }

    public class EmployeeFeedbackResponseDto
    {
        public int Id { get; set; }
        public DateOnly FeedbackDate { get; set; }

        public int DepartmentId { get; set; }
        public string DepartmentNameEn { get; set; } = string.Empty;
        public string DepartmentNameAr { get; set; } = string.Empty;

        public int? WorkerId { get; set; }
        public string? WorkerNameEn { get; set; }
        public string? WorkerNameAr { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        // "نتيجة الرضاء %": calculated, never stored
        public decimal SatisfactionPercentage { get; set; }

        public List<FeedbackAnswerResponseDto> Answers { get; set; } = new();
    }
}