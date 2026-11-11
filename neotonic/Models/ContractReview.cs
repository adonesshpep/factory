// Models/ContractReview.cs  (CHANGED - MaterialId removed; material comes from the RFQ)
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("contract_reviews")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_contract_reviews_number")]
    public class ContractReview
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        [Column("creation_date")]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        [Column("expected_delivery_date")]
        public DateTime ExpectedDate { get; set; } = DateTime.UtcNow;

        public int SupplierId { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public Supplier Supplier { get; set; } = null!;

        // Required: the material is read from this RFQ, never stored here.
        public int RFQId { get; set; }

        [ForeignKey(nameof(RFQId))]
        public RFQ RFQ { get; set; } = null!;

        [Column("connection_method")]
        public ConnectionMethods ConnectionMethod { get; set; } = ConnectionMethods.Phone;

        [Column("status")]
        public CRStatus Status { get; set; } = CRStatus.New;

        [Column("quantity", TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}