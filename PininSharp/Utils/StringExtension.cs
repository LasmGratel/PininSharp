using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PininSharp.Utils
{
    public static class StringExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SpanInRange(this string str, int startIndex, int endIndex)
        {
            return str.Substring(startIndex, endIndex - startIndex);
        }

        public static int GetUnorderedHashCode<T>(this IEnumerable<T> source)
        {
            return source.Aggregate(0, (current, element) => current ^ EqualityComparer<T>.Default.GetHashCode(element));
        }
    }
}
