using neotonic.Models;

namespace neotonic.Helpers
{
    public static class CleaningStepType
    {
        public const string KeywordEn = "cleaning";
        public const string KeywordAr = "تنظيف";

        // True if the step type's English name contains "cleaning" (any case)
        // or its Arabic name contains "تنظيف", anywhere in the text.
        public static bool Matches(StepType type) =>
            type.NameEn.Contains(KeywordEn, StringComparison.OrdinalIgnoreCase) ||
            type.NameAr.Contains(KeywordAr, StringComparison.Ordinal);
    }
}