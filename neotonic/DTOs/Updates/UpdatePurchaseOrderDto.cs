using neotonic.Models;
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdatePurchaseOrderDto
    {
        [MaxLength(50)] public string? Number { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        [Range(1, int.MaxValue)] public int? ContractReviewId { get; set; }
        [Range(1, int.MaxValue)] public int? CertificateOfAnalysisId { get; set; }

        [Range(typeof(decimal), "0.001", "999999999999999")]
        public decimal? Quantity { get; set; }
        [EnumDataType(typeof(POStatus))]
        public POStatus? Status { get; set; }

        [MaxLength(1000)] public string? Notes { get; set; }
    }
}