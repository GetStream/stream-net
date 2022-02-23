using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamNetTests
{
    internal static class Extensions
    {
        internal static IEnumerable<T> Yield<T>(this T one)
        {
            yield return one;
        }

        internal static int CountOrFallback<T>(this IEnumerable<T> list, int fallbackValue = 0)
        {
            if (list == null)
                return fallbackValue;

            return list.Count();
        }
    }
}
