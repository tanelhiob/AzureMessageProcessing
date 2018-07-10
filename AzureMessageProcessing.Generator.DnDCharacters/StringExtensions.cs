namespace AzureMessageProcessing.Generator.DnDCharacters
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns the input string with the first character converted to uppercase
        /// Source: https://stackoverflow.com/a/27073919
        /// </summary>
        public static string FirstLetterToUpperCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}
