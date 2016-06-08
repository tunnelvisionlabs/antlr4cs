// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System.Collections.Generic;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Misc
{
    /// <summary>Sometimes we need to map a key to a value but key is two pieces of data.</summary>
    /// <remarks>
    /// Sometimes we need to map a key to a value but key is two pieces of data.
    /// This nested hash table saves creating a single key each time we access
    /// map; avoids mem creation.
    /// </remarks>
    public class DoubleKeyMap<Key1, Key2, Value>
    {
        internal IDictionary<Key1, IDictionary<Key2, Value>> data = new LinkedHashMap<Key1, IDictionary<Key2, Value>>();

        public virtual Value Put(Key1 k1, Key2 k2, Value v)
        {
            IDictionary<Key2, Value> data2 = data.Get(k1);
            Value prev = null;
            if (data2 == null)
            {
                data2 = new LinkedHashMap<Key2, Value>();
                data[k1] = data2;
            }
            else
            {
                prev = data2.Get(k2);
            }
            data2[k2] = v;
            return prev;
        }

        public virtual Value Get(Key1 k1, Key2 k2)
        {
            IDictionary<Key2, Value> data2 = data.Get(k1);
            if (data2 == null)
            {
                return null;
            }
            return data2.Get(k2);
        }

        public virtual IDictionary<Key2, Value> Get(Key1 k1)
        {
            return data.Get(k1);
        }

        /// <summary>Get all values associated with primary key</summary>
        public virtual ICollection<Value> Values(Key1 k1)
        {
            IDictionary<Key2, Value> data2 = data.Get(k1);
            if (data2 == null)
            {
                return null;
            }
            return data2.Values;
        }

        /// <summary>get all primary keys</summary>
        public virtual HashSet<Key1> KeySet()
        {
            return data.Keys;
        }

        /// <summary>get all secondary keys associated with a primary key</summary>
        public virtual HashSet<Key2> KeySet(Key1 k1)
        {
            IDictionary<Key2, Value> data2 = data.Get(k1);
            if (data2 == null)
            {
                return null;
            }
            return data2.Keys;
        }
    }
}
