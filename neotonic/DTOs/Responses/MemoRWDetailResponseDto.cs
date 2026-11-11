namespace neotonic.DTOs
{
    public class MemoRWDetailResponseDto
    {
        public int Id { get; set; }
        public int MemoId { get; set; }

        public int UomId { get; set; }
        public string UomBarcode { get; set; } = string.Empty;

        public int MaterialId { get; set; }
        public string MaterialNumber { get; set; } = string.Empty;
        public string MaterialNameEn { get; set; } = string.Empty;
        public string MaterialNameAr { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public decimal Tear { get; set; }
        public decimal BWeight { get; set; }          // Quantity + Tear

        public int? ShelfId { get; set; }
        public string? ShelfLabel { get; set; }       // e.g. "I1-A03"
    }
}