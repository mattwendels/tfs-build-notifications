namespace Tfs.BuildNotifications.Common.Extensions
{
    public static class StringExtensions
    {
        public static string Shorten(this string s, int length, string suffix = null)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return string.Empty;
            }

            if (s.Length <= length)
            {
                return s;
            }

            var trimmed = s.Substring(0, length);

            return !string.IsNullOrWhiteSpace(suffix) ? trimmed + suffix : trimmed;
        }

        public static string Pluralize(this string val, int count)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                return string.Empty;
            }

            if (count == 1)
            {
                return val;
            }

            if (val.ToLower().EndsWith("s"))
            {
                return $"{val}'";
            }

            return $"{val}s";
        }
    }
}
