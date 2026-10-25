namespace neotonic.DTOs
{
    public class NextBarcodeResponseDto
    {
        // Current maximum barcode in the table; 0 if the table is empty.
        public long CurrentMaxBarcode { get; set; }
    }
}