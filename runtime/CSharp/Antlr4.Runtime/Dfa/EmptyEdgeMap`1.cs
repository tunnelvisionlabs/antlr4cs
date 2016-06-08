// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Dfa
{
    /// <summary>
    /// This implementation of
    /// <see cref="AbstractEdgeMap{T}"/>
    /// represents an empty edge map.
    /// </summary>
    /// <author>Sam Harwell</author>
    public sealed class EmptyEdgeMap<T> : AbstractEdgeMap<T>
        where T : class
    {
        public EmptyEdgeMap(int minIndex, int maxIndex)
            : base(minIndex, maxIndex)
        {
        }

        public override AbstractEdgeMap<T> Put(int key, T value)
        {
            if (value == null || key < minIndex || key > maxIndex)
            {
                // remains empty
                return this;
            }
            return new SingletonEdgeMap<T>(minIndex, maxIndex, key, value);
        }

        public override AbstractEdgeMap<T> Clear()
        {
            return this;
        }

        public override AbstractEdgeMap<T> Remove(int key)
        {
            return this;
        }

        public override int Count
        {
            get
            {
                return 0;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return true;
            }
        }

        public override bool ContainsKey(int key)
        {
            return false;
        }

        public override T this[int key]
        {
            get
            {
                return null;
            }
        }

        public override ReadOnlyDictionary<int, T> ToMap()
        {
            Dictionary<int, T> result = new Dictionary<int, T>();
            return new ReadOnlyDictionary<int, T>(result);
        }
    }
}
