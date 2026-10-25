using neotonic.Models;
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdateRFQDto
    {
        [MaxLength(50)] public string? Number { get; set; }
        [Range(1, int.MaxValue)] public int? AnnualPlanID { get; set; }

        [EnumDataType(typeof(RFQStatus))]
        public RFQStatus? Status { get; set; }

        [Range(1, int.MaxValue)] public int? MaterialId { get; set; }

        [Range(typeof(decimal), "0.001", "999999999999999")]
        public decimal? Quantity { get; set; }

        [MaxLength(1000)] public string? Notes { get; set; }
    }
}