using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace neotonic.Models
{
    [Table("supplier_samples")]
    [Index(nameof(Number), IsUnique = true, Name = "ix_supplier_samples_number")]
    public class SupplierSample
    {
        [Key]
        public int Id { get; set; }

        // Also printed as the barcode on the physical sample.
        [Required, MaxLength(50)]
        [Column("number")]
        public string Number { get; set; } = string.Empty;

        [Column("delivery_date")]
        public DateTime DeliveryDate { get; set; }

        //public int SupplierId { get; set; }

        //[ForeignKey(nameof(SupplierId))]
        //public Supplier Supplier { get; set; } = null!;

        public int ContractReviewId { get; set; }

        [ForeignKey(nameof(ContractReviewId))]
        public ContractReview ContractReview { get; set; } = null!;

        [MaxLength(1000)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}