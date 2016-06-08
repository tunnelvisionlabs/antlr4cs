// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Dfa
{
    /// <author>Sam Harwell</author>
    public interface IEdgeMap<T> : IEnumerable<KeyValuePair<int, T>>
    {
        int Count
        {
            get;
        }

        bool IsEmpty
        {
            get;
        }

        bool ContainsKey(int key);

        T this[int key]
        {
            get;
        }

        [return: NotNull]
        IEdgeMap<T> Put(int key, T value);

        [return: NotNull]
        IEdgeMap<T> Remove(int key);

        [return: NotNull]
        IEdgeMap<T> PutAll(IEdgeMap<T> m);

        [return: NotNull]
        IEdgeMap<T> Clear();

        [return: NotNull]
        ReadOnlyDictionary<int, T> ToMap();
    }
}
