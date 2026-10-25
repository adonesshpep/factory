using System.ComponentModel;
using System.Reflection;

namespace neotonic.Helpers
{
    public static class EnumExtensions
    {
        // Description format is "arabic|english".
        public static (string Ar, string En) GetBilingualNames(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var text = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;

            if (string.IsNullOrWhiteSpace(text))
                return (value.ToString(), value.ToString());

            var parts = text.Split('|', 2, StringSplitOptions.TrimEntries);
            return parts.Length == 2 ? (parts[0], parts[1]) : (text, text);
        }

        public static IEnumerable<object> GetOptions<TEnum>() where TEnum : struct, Enum =>
            Enum.GetValues<TEnum>().Select(v =>
            {
                var (ar, en) = v.GetBilingualNames();
                return new { value = Convert.ToInt32(v), nameEn = en, nameAr = ar };
            });
    }
}