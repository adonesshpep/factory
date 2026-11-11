using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdateReleaseOrderDto
    {
        [MaxLength(50)] public string? Number { get; set; }
        public DateTime? OrderDate { get; set; }
        public int? ManufacturingOrderId { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
    }
}