using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdateSupplierSampleDto
    {
        [MaxLength(50)]
        [RegularExpression(SupplierSampleRules.NumberPattern, ErrorMessage = SupplierSampleRules.NumberMessage)]
        public string? Number { get; set; }

        public DateTime? DeliveryDate { get; set; }

        //[Range(1, int.MaxValue)] public int? SupplierId { get; set; }
        [Range(1, int.MaxValue)] public int? ContractReviewId { get; set; }

        [MaxLength(1000)] public string? Notes { get; set; }
    }
}