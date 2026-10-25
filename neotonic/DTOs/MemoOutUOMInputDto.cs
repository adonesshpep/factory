// DTOs/MemoOutUOMInputDto.cs
using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class MemoOutUOMInputDto
    {
        [Required, Range(1, int.MaxValue)]
        public int UOMId { get; set; }

        [Required, Range(typeof(decimal), "0.001", "999999999999999")]
        public decimal Quantity { get; set; }
    }
}