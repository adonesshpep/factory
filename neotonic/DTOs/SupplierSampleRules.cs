namespace neotonic.DTOs
{
    public static class SupplierSampleRules
    {
        // Barcode-safe: ASCII letters, digits, '.', '-', '_' and no spaces.
        // For Code 39, remove '_' from the pattern.
        public const string NumberPattern = @"^[A-Za-z0-9._-]+$";
        public const string NumberMessage =
            "Number may only contain English letters, digits, '.', '-' and '_' (it is printed as a barcode)";
    }
}