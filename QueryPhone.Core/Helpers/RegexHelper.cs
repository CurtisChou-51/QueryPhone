using System.Text.RegularExpressions;

namespace QueryPhone.Core.Helpers
{
    public static partial class RegexHelper
    {
        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();

        public static string RemoveWhitespace(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
            
            return WhitespaceRegex().Replace(input, string.Empty);
        }

        public static string CollapseWhitespace(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
            
            return WhitespaceRegex().Replace(input, " ");
        }
    }
}
