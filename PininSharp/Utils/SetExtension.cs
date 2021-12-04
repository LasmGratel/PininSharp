using System.Collections.Generic;

namespace PininSharp.Utils
{
    public static class SetExtension
    {
        public static void AddAll<T>(this ICollection<T> collection, IEnumerable<T> other)
        {
            foreach (var value in other)
            {
                collection.Add(value);
            }
        }
    }
}
