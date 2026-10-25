// DTOs/UOMResponseDto.cs
namespace neotonic.DTOs
{
    public class UOMResponseDto
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public int BoxNumber { get; set; }
        public int PrintCount { get; set; }
        public decimal Quantity { get; set; }
    }
}