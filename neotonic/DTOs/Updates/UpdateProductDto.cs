using System.ComponentModel.DataAnnotations;

namespace neotonic.DTOs.Updates
{
    public class UpdateProductDto
    {
        [MaxLength(200)] public string? NameEn { get; set; }
        [MaxLength(200)] public string? NameAr { get; set; }

        [MaxLength(50)] public string? ProductNumber { get; set; }
        [MaxLength(50)] public string? AccountingCode { get; set; }


        public string? Notes { get; set; }

        [Range(1, int.MaxValue)] public int? CategoryId { get; set; }
        public bool? IsSerialized { get; set; }
    }
}
