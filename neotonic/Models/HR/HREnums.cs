using System.ComponentModel;

namespace neotonic.Models.HR
{
    // The numeric value is the score printed on the paper form
    public enum FeedbackScore
    {
        [Description("ضعيف|Weak")] Weak = 2,
        [Description("وسط|Average")] Average = 4,
        [Description("جيد|Good")] Good = 6,
        [Description("جيد جدا|Very Good")] VeryGood = 8,
        [Description("ممتاز|Excellent")] Excellent = 10
    }
}