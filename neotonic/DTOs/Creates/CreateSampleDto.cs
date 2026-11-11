using neotonic.Models;
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateSampleDto
    {
        [Required, MaxLength(50)]
        [RegularExpression(SupplierSampleRules.NumberPattern, ErrorMessage = SupplierSampleRules.NumberMessage)]
        public string Number { get; set; } = string.Empty;

        [Required, EnumDataType(typeof(SampleTypes))]
        public SampleTypes? SampleType { get; set; }

        [Required]
        public DateTime? SampleDate { get; set; }

        // Supplier sample -> ContractReviewId required, UomId must be null
        [Range(1, int.MaxValue)]
        public int? ContractReviewId { get; set; }

        // Delivery sample -> UomId required, ContractReviewId must be null
        [Range(1, int.MaxValue)]
        public int? UomId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}