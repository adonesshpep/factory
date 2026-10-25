using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs
{
    // One row in the "update all details of a memo" payload.
    // Id identifies the existing detail; if Barcode is sent and differs, the
    // controller re-validates global uniqueness before saving.
    public class UpdateMemoRWDetailItemDto
    {
        [Required, Range(1, int.MaxValue)]
        public int Id { get; set; }

        public long? Barcode { get; set; }
        public decimal? AWeight { get; set; }
        public decimal? BWeight { get; set; }
    }
}