using System.Text.RegularExpressions;

namespace R.Editor
{
    public static class NameUtils
    {
        public static string ToName(string text)
        {
            if (false == string.IsNullOrEmpty(text))
            {
                var result = Regex.Replace(text, "[^a-zA-Z0-9]", "_");
                if ('0' <= result[0] && result[0] <= '9')
                    result = result.Insert(0, "_");
                return result;
            }
            return string.Empty;
        }
    }
}
