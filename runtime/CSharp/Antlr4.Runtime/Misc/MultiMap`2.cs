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

namespace Antlr4.Runtime.Misc
{
    [System.Serializable]
    public class MultiMap<K, V> : LinkedHashMap<K, IList<V>>
    {
        private const long serialVersionUID = -4956746660057462312L;

        public virtual void Map(K key, V value)
        {
            IList<V> elementsForKey = this[key];
            if (elementsForKey == null)
            {
                elementsForKey = new List<V>();
                base.Put;
            }
            elementsForKey.Add(value);
        }

        public virtual IList<Tuple<K, V>> GetPairs()
        {
            IList<Tuple<K, V>> pairs = new List<Tuple<K, V>>();
            foreach (K key in Keys)
            {
                foreach (V value in this[key])
                {
                    pairs.Add(Tuple.Create(key, value));
                }
            }
            return pairs;
        }
    }
}
