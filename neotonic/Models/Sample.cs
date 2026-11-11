// Models/Sample.cs  (CHANGED)
// Supplier sample -> ContractReviewId set, UomId null
// Delivery sample -> UomId set, ContractReviewId null
// check constraint enforces exactly that, matching SampleType.
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("samples")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_samples_number")]
    [Index(nameof(ContractReviewId), Name = "ix_samples_contract_review")]
    [Index(nameof(UomId), Name = "ix_samples_uom")]
    public class Sample
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        [Column("sample_type")]
        public SampleTypes SampleType { get; set; }

        [Column("sample_date")]
        public DateTime SampleDate { get; set; }

        public int? ContractReviewId { get; set; }

        [ForeignKey(nameof(ContractReviewId))]
        public ContractReview? ContractReview { get; set; }

        public int? UomId { get; set; }

        [ForeignKey(nameof(UomId))]
        public UOM? Uom { get; set; }

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}