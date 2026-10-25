using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    // Whole list is replaced/updated in one call (per your requirement).
    // Rows not included in the list are left untouched; use the delete-detail
    // endpoint to remove a row.
    public class UpdateMemoRWDetailsDto
    {
        [Required, MinLength(1)]
        public List<UpdateMemoRWDetailItemDto> Details { get; set; } = new();
    }
}