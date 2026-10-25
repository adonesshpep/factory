namespace neotonic.DTOs
{
    // Returned from the "add one detail" endpoint so the client can immediately
    // show the freshly-inserted row and the current max barcode in the table.
    public class AddMemoRWDetailResponseDto
    {
        public MemoRWDetailResponseDto Detail { get; set; } = null!;
        public long CurrentMaxBarcode { get; set; }
    }
}