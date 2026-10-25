using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreatePurchaseOrderDto
    {
        [Required, MaxLength(50)]
        public string Number { get; set; } = string.Empty;

        public DateTime? CreationDate { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int ContractReviewId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int CertificateOfAnalysisId { get; set; }

        [Required, Range(typeof(decimal), "0.001", "999999999999999")]
        public decimal? Quantity { get; set; }

        [Required]
        public DateTime? DeliveryDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
