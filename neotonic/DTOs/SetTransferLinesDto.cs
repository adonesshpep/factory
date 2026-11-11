using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class SetTransferLinesDto
    {
        [Required, MinLength(1)]
        public List<TransferLineInputDto> Lines { get; set; } = new();
    }

    public class TransferLineInputDto
    {
        [Required, Range(1, int.MaxValue)] public int UomId { get; set; }
        [Required, Range(typeof(decimal), "0.001", "999999999999999")] public decimal Quantity { get; set; }
    }
}