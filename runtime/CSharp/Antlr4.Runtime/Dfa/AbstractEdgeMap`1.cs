// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* Copyright (c) 2012 The ANTLR Project. All rights reserved.
* Use of this file is governed by the BSD-3-Clause license that
* can be found in the LICENSE.txt file in the project root.
*/
using System.Collections.Generic;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Dfa
{
    /// <author>Sam Harwell</author>
    public abstract class AbstractEdgeMap<T> : IEdgeMap<T>
    {
        protected internal readonly int minIndex;

        protected internal readonly int maxIndex;

        public AbstractEdgeMap(int minIndex, int maxIndex)
        {
            // the allowed range (with minIndex and maxIndex inclusive) should be less than 2^32
            System.Diagnostics.Debug.Assert(maxIndex - minIndex + 1 >= 0);
            this.minIndex = minIndex;
            this.maxIndex = maxIndex;
        }

        public abstract Antlr4.Runtime.Dfa.AbstractEdgeMap<T> Put(int key, T value);

        public virtual Antlr4.Runtime.Dfa.AbstractEdgeMap<T> PutAll<_T0>(IEdgeMap<_T0> m)
            where _T0 : T
        {
            Antlr4.Runtime.Dfa.AbstractEdgeMap<T> result = this;
            foreach (KeyValuePair<int, T> entry in m.EntrySet())
            {
                result = result.Put(entry.Key, entry.Value);
            }
            return result;
        }

        public abstract Antlr4.Runtime.Dfa.AbstractEdgeMap<T> Clear();

        public abstract Antlr4.Runtime.Dfa.AbstractEdgeMap<T> Remove(int key);

        public abstract bool ContainsKey(int arg1);

        public abstract T Get(int arg1);

        public abstract bool IsEmpty
        {
            get;
        }

        public abstract int Count
        {
            get;
        }

        public abstract IDictionary<int, T> ToMap();
    }
}
