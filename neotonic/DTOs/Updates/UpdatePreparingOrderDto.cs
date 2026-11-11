using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdatePreparingOrderDto
    {
        [MaxLength(50)] public string? Number { get; set; }
        public DateTime? OrderDate { get; set; }
        [Range(1, int.MaxValue)] public int? ProtocolId { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
    }
}