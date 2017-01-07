// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* Copyright (c) 2012 The ANTLR Project. All rights reserved.
* Use of this file is governed by the BSD-3-Clause license that
* can be found in the LICENSE.txt file in the project root.
*/
using System;
using System.Collections.Generic;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Dfa
{
    /// <author>Sam Harwell</author>
    public sealed class ArrayEdgeMap<T> : AbstractEdgeMap<T>
    {
        private readonly AtomicReferenceArray<T> arrayData;

        private readonly AtomicInteger size;

        public ArrayEdgeMap(int minIndex, int maxIndex)
            : base(minIndex, maxIndex)
        {
            arrayData = new AtomicReferenceArray<T>(maxIndex - minIndex + 1);
            size = new AtomicInteger();
        }

        public override int Count
        {
            get
            {
                return size.Get();
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return Count == 0;
            }
        }

        public override bool ContainsKey(int key)
        {
            return this[key] != null;
        }

        public override T this[int key]
        {
            get
            {
                if (key < minIndex || key > maxIndex)
                {
                    return null;
                }
                return arrayData.Get(key - minIndex);
            }
        }

        public override AbstractEdgeMap<T> Put(int key, T value)
        {
            if (key >= minIndex && key <= maxIndex)
            {
                T existing = arrayData.GetAndSet(key - minIndex, value);
                if (existing == null && value != null)
                {
                    size.IncrementAndGet();
                }
                else
                {
                    if (existing != null && value == null)
                    {
                        size.DecrementAndGet();
                    }
                }
            }
            return this;
        }

        public override AbstractEdgeMap<T> Remove(int key)
        {
            return ((Antlr4.Runtime.Dfa.ArrayEdgeMap<T>)Put(key, null));
        }

        public override AbstractEdgeMap<T> PutAll<_T0>(IEdgeMap<_T0> m)
        {
            if (m.IsEmpty)
            {
                return this;
            }
            if (m is Antlr4.Runtime.Dfa.ArrayEdgeMap<object>)
            {
                Antlr4.Runtime.Dfa.ArrayEdgeMap<T> other = (Antlr4.Runtime.Dfa.ArrayEdgeMap<T>)m;
                int minOverlap = Math.Max(minIndex, other.minIndex);
                int maxOverlap = Math.Min(maxIndex, other.maxIndex);
                Antlr4.Runtime.Dfa.ArrayEdgeMap<T> result = this;
                for (int i = minOverlap; i <= maxOverlap; i++)
                {
                    result = ((Antlr4.Runtime.Dfa.ArrayEdgeMap<T>)result.Put(i, m[i]));
                }
                return result;
            }
            else
            {
                if (m is SingletonEdgeMap<object>)
                {
                    SingletonEdgeMap<T> other = (SingletonEdgeMap<T>)m;
                    System.Diagnostics.Debug.Assert(!other.IsEmpty);
                    return ((Antlr4.Runtime.Dfa.ArrayEdgeMap<T>)Put(other.Key, other.Value));
                }
                else
                {
                    if (m is SparseEdgeMap<object>)
                    {
                        SparseEdgeMap<T> other = (SparseEdgeMap<T>)m;
                        lock (other)
                        {
                            int[] keys = other.Keys;
                            IList<T> values = other.Values;
                            Antlr4.Runtime.Dfa.ArrayEdgeMap<T> result = this;
                            for (int i = 0; i < values.Count; i++)
                            {
                                result = ((Antlr4.Runtime.Dfa.ArrayEdgeMap<T>)result.Put(keys[i], values[i]));
                            }
                            return result;
                        }
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("EdgeMap of type %s is supported yet.", m.GetType().FullName));
                    }
                }
            }
        }

        public override AbstractEdgeMap<T> Clear()
        {
            return new EmptyEdgeMap<T>(minIndex, maxIndex);
        }

        public override IDictionary<int, T> ToMap()
        {
            if (IsEmpty)
            {
                return Antlr4.Runtime.Sharpen.Collections.EmptyMap();
            }
            IDictionary<int, T> result = new LinkedHashMap<int, T>();
            for (int i = 0; i < arrayData.Length(); i++)
            {
                T element = arrayData.Get(i);
                if (element == null)
                {
                    continue;
                }
                result[i + minIndex] = element;
            }
            return result;
        }
    }
}
