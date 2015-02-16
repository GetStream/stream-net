using System;
using System.Collections.Generic;
using System.Linq;

namespace Stream
{
    public static class Extensions
    {
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> input)
        {
            if (input == null) return Enumerable.Empty<T>();
            return input;
        }

        public static IEnumerable<T> Yield<T>(this T one)
        {
            yield return one;
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if ((items == null) || (action == null)) return; // do nothing
            foreach (var item in items)
                action(item);
        }

        public static int SafeCount<T>(this IEnumerable<T> list, int nullCountAs = 0)
        {
            if (list == null) return nullCountAs;
            return list.Count();
        }
    }
}
