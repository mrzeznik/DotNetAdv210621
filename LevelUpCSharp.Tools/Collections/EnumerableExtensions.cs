using System;
using System.Collections.Generic;
using System.Text;

namespace LevelUpCSharp.Collections
{
    public static class EnumerableExtensions
    {
        public static void ForEach<TElement>(this IEnumerable<TElement> source, Action<TElement> elementAction)
        {
            foreach (var element in source)
            {
                elementAction(element);
            }
        }
    }
}
