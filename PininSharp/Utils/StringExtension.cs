using System.Runtime.CompilerServices;

namespace PininSharp.Utils
{
    public static class StringExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringInRange(this string str, int startIndex, int endIndex)
        {
            return str.Substring(startIndex, endIndex - startIndex);
        }
    }
}
