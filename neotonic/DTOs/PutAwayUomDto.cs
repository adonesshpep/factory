using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    public class PutAwayUomDto
    {
        [Required, Range(1, int.MaxValue)]
        public int ShelfId { get; set; }
    }
}