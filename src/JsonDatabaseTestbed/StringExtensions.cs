using System;
using System.Linq;

namespace JsonDatabaseTestbed
{
    public static partial class StringExtensions
    {
        public static bool IsRoughly(this string input, params string[] matches)
        {
            return matches.Any(match => input.Trim().Equals(match.Trim(), StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool RoughlyStartsWith(this string input, string startsWith)
        {
            return input.StartsWith(startsWith, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}