using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class FinishTransferOrderDto
    {
        [Required]
        public List<int> ScannedUomIds { get; set; } = new();
    }
}