using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("workers")]
    [Index(nameof(WorkerNumber), IsUnique = true, Name = "ix_workers_worker_number")]
    public class Worker
    {
        [Key]
        public int Id { get; set; }

        //(see AppDbContext before calling me 😒).
        [Column("worker_number")]
        public int WorkerNumber { get; set; }

        [Required, MaxLength(200)]
        [Column("name_en")]
        public string NameEn { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [Column("name_ar")]
        public string NameAr { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        [Column("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Column("birth_date")]
        public DateOnly BirthDate { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}