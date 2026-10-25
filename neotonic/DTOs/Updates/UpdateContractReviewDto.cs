using neotonic.Models;
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdateContractReviewDto
    {
        [MaxLength(50)] public string? Number { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ExpectedDate { get; set; }

        [Range(1, int.MaxValue)] public int? SupplierId { get; set; }
        [EnumDataType(typeof(ConnectionMethods))] public ConnectionMethods? ConnectionMethod { get; set; }
        [Range(1, int.MaxValue)] public int? RFQId { get; set; }
        [Range(1, int.MaxValue)] public int? MaterialId { get; set; }

        [Range(typeof(decimal), "0.001", "999999999999999")]
        public decimal? Quantity { get; set; }
        [EnumDataType(typeof(CRStatus))]
        public CRStatus? Status { get; set; }

        [MaxLength(1000)] public string? Notes { get; set; }
    }
}