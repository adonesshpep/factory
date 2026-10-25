namespace neotonic.Helpers
{
    public static class BilingualFieldResolver
    {
        /// <summary>
        /// Resolves an EN/AR field pair: if only one is provided, copies it into the other.
        /// Returns false if both are missing (caller should reject the request).
        /// </summary>
        public static bool TryResolve(string? enInput, string? arInput, out string enResolved, out string arResolved)
        {
            var hasEn = !string.IsNullOrWhiteSpace(enInput);
            var hasAr = !string.IsNullOrWhiteSpace(arInput);

            if (!hasEn && !hasAr)
            {
                enResolved = string.Empty;
                arResolved = string.Empty;
                return false;
            }

            enResolved = hasEn ? enInput! : arInput!;
            arResolved = hasAr ? arInput! : enInput!;
            return true;
        }
        }
    }
