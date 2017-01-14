// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Misc
{
    using System.Collections.Generic;

    /** I need the get-element-i functionality so I'm subclassing
     *  LinkedHashMap.
     */
    public class OrderedHashMap<K, V> : LinkedHashMap<K, V>
    {
        /** Track the elements as they are added to the set */
        protected IList<K> elements = new List<K>();

        public override V this[K key]
        {
            get
            {
                return base[key];
            }

            set
            {
                elements.Add(key);
                base[key] = value;
            }
        }

        public virtual K GetKey(int i)
        {
            return elements[i];
        }

        public virtual V GetElement(int i)
        {
            V result;
            TryGetValue(GetKey(i), out result);
            return result;
        }

        public override void Add(K key, V value)
        {
            base.Add(key, value);
            elements.Add(key);
        }

        //public override void PutAll(Map<K, V> m)
        //{
        //    foreach (Map.Entry<K, V> entry in m.entrySet())
        //    {
        //        put(entry.getKey(), entry.getValue());
        //    }
        //}

        public override bool Remove(K key)
        {
            elements.Remove(key);
            return base.Remove(key);
        }

        public override void Clear()
        {
            elements.Clear();
            base.Clear();
        }
    }
}
