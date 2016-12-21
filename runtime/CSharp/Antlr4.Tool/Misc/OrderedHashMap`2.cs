/*
 * [The "BSD license"]
 *  Copyright (c) 2012 Terence Parr
 *  Copyright (c) 2012 Sam Harwell
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *  1. Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 *  3. The name of the author may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 *  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 *  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 *  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 *  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 *  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 *  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 *  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
