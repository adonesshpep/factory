// Helpers/PreparingStepType.cs — mirrors CleaningStepType
using neotonic.Models;

namespace neotonic.Helpers
{
    public static class PreparingStepType
    {
        public const string KeywordEn = "preparing";
        public const string KeywordAr = "تجهيز";

        public static bool Matches(StepType type) =>
            type.NameEn.Contains(KeywordEn, StringComparison.OrdinalIgnoreCase) ||
            type.NameAr.Contains(KeywordAr, StringComparison.Ordinal);
    }
}