using System;
using System.Collections.Generic;
using System.Linq;

namespace Stream.Utils
{
    internal static class Extensions
    {
        internal static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if ((items == null) || (action == null))
                return;

            foreach (var item in items)
                action(item);
        }

        internal static int CountOrFallback<T>(this IEnumerable<T> list, int fallbackValue = 0)
        {
            if (list == null)
                return fallbackValue;

            return list.Count();
        }
    }
}
