using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using neotonic.DTOs;
using neotonic.DTOs.HR;
using neotonic.Helpers;
using neotonic.Models.HR;
// plus the usings for AppDbContext and your shared ChangeStatusDto

namespace neotonic.Controllers.HR
{
    [ApiController]
    [Route("api/hr/employee-feedbacks")]
    [Authorize(Roles = "Admin,Manager")] // adjust to the roles that may see HR data
    public class EmployeeFeedbacksController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EmployeeFeedbacksController(AppDbContext db) => _db = db;

        // The 10 fixed questions, so the front end can render the form
        [HttpGet("questions")]
        public IActionResult GetQuestions() => Ok(FeedbackQuestions.All);

        // Excellent / Very Good / Good / Average / Weak with their values
        [HttpGet("score-options")]
        public IActionResult GetScoreOptions() => Ok(EnumExtensions.GetOptions<FeedbackScore>());

        [HttpGet]
        public async Task<ActionResult<List<EmployeeFeedbackResponseDto>>> GetAll(
            [FromQuery] int? departmentId, [FromQuery] int? workerId,
            [FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
        {
            var query = FullQuery();

            if (departmentId.HasValue) query = query.Where(f => f.DepartmentId == departmentId);
            if (workerId.HasValue) query = query.Where(f => f.WorkerId == workerId);
            if (from.HasValue) query = query.Where(f => f.FeedbackDate >= from);
            if (to.HasValue) query = query.Where(f => f.FeedbackDate <= to);

            var list = await query.OrderByDescending(f => f.FeedbackDate)
                .ThenByDescending(f => f.Id).ToListAsync();

            return Ok(list.Select(ToDto));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmployeeFeedbackResponseDto>> GetById(int id)
        {
            var dto = await LoadDto(id);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<EmployeeFeedbackResponseDto>> Create(SaveEmployeeFeedbackDto dto)
        {
            var error = await Validate(dto);
            if (error != null) return BadRequest(new { message = error });

            var entity = new EmployeeFeedback
            {
                FeedbackDate = dto.FeedbackDate ?? DateOnly.FromDateTime(DateTime.Today),
                DepartmentId = dto.DepartmentId,
                WorkerId = dto.WorkerId,
                JobTitle = dto.JobTitle.Trim(),
                Answers = dto.Answers.Select(a => new EmployeeFeedbackAnswer
                {
                    QuestionNumber = a.QuestionNumber,
                    Score = a.Score,
                    Reason = NormalizeText(a.Reason)
                }).ToList()
            };

            _db.EmployeeFeedbacks.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, await LoadDto(entity.Id));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<EmployeeFeedbackResponseDto>> Update(int id, SaveEmployeeFeedbackDto dto)
        {
            var entity = await _db.EmployeeFeedbacks.Include(f => f.Answers)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (entity is null) return NotFound();

            var error = await Validate(dto);
            if (error != null) return BadRequest(new { message = error });

            entity.FeedbackDate = dto.FeedbackDate ?? entity.FeedbackDate;
            entity.DepartmentId = dto.DepartmentId;
            entity.WorkerId = dto.WorkerId;
            entity.JobTitle = dto.JobTitle.Trim();

            // Validate guarantees questions 1..10 are each present once,
            // so the existing answer rows are edited in place.
            foreach (var answer in entity.Answers)
            {
                var incoming = dto.Answers.First(a => a.QuestionNumber == answer.QuestionNumber);
                answer.Score = incoming.Score;
                answer.Reason = NormalizeText(incoming.Reason);
            }

            await _db.SaveChangesAsync();
            return Ok(await LoadDto(id));
        }

        // Soft delete toggle
        [HttpPut("status/{id:int}")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeStatusDto dto)
        {
            var entity = await _db.EmployeeFeedbacks.FindAsync(id);
            if (entity is null) return NotFound();

            entity.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Hard delete (answers are removed by cascade)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.EmployeeFeedbacks.FindAsync(id);
            if (entity is null) return NotFound();

            _db.EmployeeFeedbacks.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------- helpers ----------

        private IQueryable<EmployeeFeedback> FullQuery() =>
            _db.EmployeeFeedbacks.AsNoTracking()
                .Include(f => f.Department)
                .Include(f => f.Worker)
                .Include(f => f.Answers);

        private async Task<string?> Validate(SaveEmployeeFeedbackDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.JobTitle))
                return "JobTitle is required.";

            if (!await _db.Departments.AnyAsync(d => d.Id == dto.DepartmentId))
                return "Department not found.";

            if (dto.WorkerId.HasValue && !await _db.Workers.AnyAsync(w => w.Id == dto.WorkerId))
                return "Worker not found.";

            var expected = FeedbackQuestions.All.Count;
            if (dto.Answers is null || dto.Answers.Count != expected)
                return $"Exactly {expected} answers are required, one per question.";

            var numbers = dto.Answers.Select(a => a.QuestionNumber).ToHashSet();
            if (numbers.Count != expected || !FeedbackQuestions.All.All(q => numbers.Contains(q.Number)))
                return $"Each question number (1-{expected}) must be answered exactly once.";

            if (dto.Answers.Any(a => !Enum.IsDefined(a.Score)))
                return "Score must be one of 2, 4, 6, 8, 10.";

            // Not enforced: a reason is required when the score is Average or Weak.
            // To enforce it, uncomment:
            // if (dto.Answers.Any(a => a.Score <= FeedbackScore.Average && string.IsNullOrWhiteSpace(a.Reason)))
            //     return "A reason is required for every Average or Weak answer.";

            return null;
        }

        private static string? NormalizeText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private async Task<EmployeeFeedbackResponseDto?> LoadDto(int id)
        {
            var entity = await FullQuery().FirstOrDefaultAsync(f => f.Id == id);
            return entity is null ? null : ToDto(entity);
        }

        private static EmployeeFeedbackResponseDto ToDto(EmployeeFeedback f)
        {
            var answers = f.Answers.OrderBy(a => a.QuestionNumber).Select(a =>
            {
                var question = FeedbackQuestions.All.First(q => q.Number == a.QuestionNumber);
                var (scoreAr, scoreEn) = a.Score.GetBilingualNames();
                return new FeedbackAnswerResponseDto
                {
                    QuestionNumber = a.QuestionNumber,
                    QuestionTextAr = question.TextAr,
                    QuestionTextEn = question.TextEn,
                    Score = (int)a.Score,
                    ScoreNameAr = scoreAr,
                    ScoreNameEn = scoreEn,
                    Reason = a.Reason
                };
            }).ToList();

            var maxScore = answers.Count * 10;

            return new EmployeeFeedbackResponseDto
            {
                Id = f.Id,
                FeedbackDate = f.FeedbackDate,
                DepartmentId = f.DepartmentId,
                DepartmentNameEn = f.Department.NameEn,
                DepartmentNameAr = f.Department.NameAr,
                WorkerId = f.WorkerId,
                WorkerNameEn = f.Worker?.NameEn,
                WorkerNameAr = f.Worker?.NameAr,
                JobTitle = f.JobTitle,
                IsActive = f.IsActive,
                SatisfactionPercentage = maxScore == 0
                    ? 0
                    : Math.Round(answers.Sum(a => a.Score) * 100m / maxScore, 2),
                Answers = answers
            };
        }
    }
}