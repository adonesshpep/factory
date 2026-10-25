namespace neotonic.DTOs
{
    public class MemoRWDetailResponseDto
    {
        public int Id { get; set; }
        public int MemoId { get; set; }
        public long Barcode { get; set; }
        public decimal AWeight { get; set; }
        public decimal BWeight { get; set; }
    }
}