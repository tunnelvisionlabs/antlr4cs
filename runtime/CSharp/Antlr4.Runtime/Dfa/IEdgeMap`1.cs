// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/* Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Dfa
{
    /// <author>Sam Harwell</author>
    public interface IEdgeMap<T>
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

        [NotNull]
        IEdgeMap<T> Put(int key, [Nullable] T value);

        [NotNull]
        IEdgeMap<T> Remove(int key);

        [NotNull]
        IEdgeMap<T> PutAll<_T0>(IEdgeMap<_T0> m)
            where _T0 : T;

        [NotNull]
        IEdgeMap<T> Clear();

        [NotNull]
        IDictionary<int, T> ToMap();
    }
}
