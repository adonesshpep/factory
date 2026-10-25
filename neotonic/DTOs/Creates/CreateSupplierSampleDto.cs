using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateSupplierSampleDto
    {
        [Required, MaxLength(50)]
        [RegularExpression(SupplierSampleRules.NumberPattern, ErrorMessage = SupplierSampleRules.NumberMessage)]
        public string Number { get; set; } = string.Empty;

        [Required]
        public DateTime? DeliveryDate { get; set; }

        //[Required, Range(1, int.MaxValue)]
        //public int SupplierId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int ContractReviewId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}