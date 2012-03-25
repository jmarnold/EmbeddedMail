namespace EmbeddedMail.Handlers
{
    public static class StringExtensions
    {
        public static string ValueFromAttributeSyntax(this string input)
        {
            return input.Split(":".ToCharArray())[1].Trim();
        }
    }
}