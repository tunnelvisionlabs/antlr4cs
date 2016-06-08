// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Misc
{
    [System.Serializable]
    public class MultiMap<K, V> : Dictionary<K, IList<V>>
    {
        private const long serialVersionUID = -4956746660057462312L;

        public virtual void Map(K key, V value)
        {
            IList<V> elementsForKey;
            if (!TryGetValue(key, out elementsForKey))
            {
                elementsForKey = new List<V>();
                this[key] = elementsForKey;
            }
            elementsForKey.Add(value);
        }

        public virtual IList<Tuple<K, V>> GetPairs()
        {
            IList<Tuple<K, V>> pairs = new List<Tuple<K, V>>();
            foreach (KeyValuePair<K, IList<V>> pair in this)
            {
                foreach (V value in pair.Value)
                {
                    pairs.Add(Tuple.Create(pair.Key, value));
                }
            }

            return pairs;
        }
    }
}
