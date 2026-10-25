using neotonic.Models;
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Creates
{
    public class CreateContractReviewDto
    {
        [Required, MaxLength(50)]
        public string Number { get; set; } = string.Empty;

        public DateTime? CreationDate { get; set; }

        [Required]
        public DateTime? ExpectedDate { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int SupplierId { get; set; }

        [EnumDataType(typeof(ConnectionMethods))]
        public ConnectionMethods? ConnectionMethod { get; set; }
        [Required, Range(1, int.MaxValue)]
        public int RFQId { get; set; }
        [Required, Range(1, int.MaxValue)]
        public int MaterialId { get; set; }

        [Required, Range(typeof(decimal), "0.001", "999999999999999")]
        public decimal? Quantity { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}