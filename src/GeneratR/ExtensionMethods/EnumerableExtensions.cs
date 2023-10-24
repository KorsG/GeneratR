using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneratR.ExtensionMethods;

internal static class EnumerableExtensions
{
    internal static bool In<T>(this T value, IEnumerable<T> sequence)
    {
        return sequence.Contains(value);
    }

    internal static bool In<T>(this T value, params T[] sequence)
    {
        return sequence.Contains(value);
    }
}
