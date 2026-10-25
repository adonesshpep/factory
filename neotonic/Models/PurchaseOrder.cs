using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("purchase_orders")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_purchase_orders_number")]
    public class PurchaseOrder
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        [Column("creation_date")]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public int ContractReviewId { get; set; }

        [ForeignKey(nameof(ContractReviewId))]
        public ContractReview ContractReview { get; set; } = null!;

        public int CertificateOfAnalysisId { get; set; }

        [ForeignKey(nameof(CertificateOfAnalysisId))]
        public CertificateOfAnalysis CertificateOfAnalysis { get; set; } = null!;

        [Column("quantity", TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column("delivery_date")]
        public DateTime DeliveryDate { get; set; }

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("status")]
        public POStatus Status { get; set; } = POStatus.Pending;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}