// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Runtime.Sharpen
{
    using System.Collections.Generic;

    internal static class ListExtensions
    {
        public static T Set<T>(this IList<T> list, int index, T value)
            where T : class
        {
            T previous = list[index];
            list[index] = value;
            return previous;
        }
    }
}
