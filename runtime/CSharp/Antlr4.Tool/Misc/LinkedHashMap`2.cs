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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class LinkedHashMap<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
    {
        private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _dictionary;
        private readonly LinkedList<KeyValuePair<TKey, TValue>> _list;

        public LinkedHashMap()
        {
            _dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
            _list = new LinkedList<KeyValuePair<TKey, TValue>>();
        }

        public LinkedHashMap(IEnumerable<KeyValuePair<TKey, TValue>> items)
            : this()
        {
            foreach (var item in items)
                Add(item.Key, item.Value);
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                return _dictionary[key].Value.Value;
            }

            set
            {
                LinkedListNode<KeyValuePair<TKey, TValue>> node;
                if (_dictionary.TryGetValue(key, out node))
                {
                    node.Value = new KeyValuePair<TKey, TValue>(node.Value.Key, value);
                }
                else
                {
                    node = _list.AddLast(new KeyValuePair<TKey, TValue>(key, value));
                    _dictionary[key] = node;
                }
            }
        }

        public virtual int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual ICollection<TKey> Keys
        {
            get
            {
                return new KeyCollection(this);
            }
        }

        public virtual ICollection<TValue> Values
        {
            get
            {
                return new ValueCollection(this);
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return new KeyCollection(this);
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return new ValueCollection(this);
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public object this[object key]
        {
            get
            {
                if (!(key is TKey))
                {
                    if (!(key == (object)default(TKey)))
                    {
                        return null;
                    }
                }

                TValue result;
                if (!TryGetValue((TKey)key, out result))
                    return null;

                return result;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public virtual void Add(TKey key, TValue value)
        {
            var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key, value));
            _dictionary.Add(key, node);
            _list.AddLast(node);
        }

        public virtual void Clear()
        {
            _dictionary.Clear();
            _list.Clear();
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _list.Contains(item);
        }

        public virtual bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public virtual bool Remove(TKey key)
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> node;
            if (!_dictionary.TryGetValue(key, out node))
                return false;

            _dictionary.Remove(key);
            _list.Remove(node);
            return true;
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> node;
            if (!_dictionary.TryGetValue(key, out node))
            {
                value = default(TValue);
                return false;
            }

            value = node.Value.Value;
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void IDictionary.Add(object key, object value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary.Contains(object key)
        {
            if (!(key is TKey))
            {
                if (!(key == (object)default(TKey)))
                {
                    return false;
                }
            }

            TValue result;
            return TryGetValue((TKey)key, out result);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator(GetEnumerator());
        }

        void IDictionary.Remove(object key)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        private class KeyCollection : ICollection<TKey>, ICollection
        {
            private readonly LinkedHashMap<TKey, TValue> _map;

            public KeyCollection(LinkedHashMap<TKey, TValue> map)
            {
                _map = map;
            }

            public int Count
            {
                get
                {
                    return _map.Count;
                }
            }

            bool ICollection<TKey>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    throw new NotSupportedException();
                }
            }

            void ICollection<TKey>.Add(TKey item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                _map.Clear();
            }

            public bool Contains(TKey item)
            {
                return _map.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                TKey[] keys = this.ToArray();
                keys.CopyTo(array, arrayIndex);
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                return _map.Select(pair => pair.Key).GetEnumerator();
            }

            public bool Remove(TKey item)
            {
                return _map.Remove(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            void ICollection.CopyTo(Array array, int index)
            {
                TKey[] keys = this.ToArray();
                keys.CopyTo(array, index);
            }
        }

        public class ValueCollection : ICollection<TValue>, ICollection
        {
            private readonly LinkedHashMap<TKey, TValue> _map;

            public ValueCollection(LinkedHashMap<TKey, TValue> map)
            {
                _map = map;
            }

            public int Count
            {
                get
                {
                    return _map.Count;
                }
            }

            bool ICollection<TValue>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    throw new NotSupportedException();
                }
            }

            void ICollection<TValue>.Add(TValue item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                _map.Clear();
            }

            bool ICollection<TValue>.Contains(TValue item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                TValue[] values = this.ToArray();
                values.CopyTo(array, arrayIndex);
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                return _map.Select(pair => pair.Value).GetEnumerator();
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            void ICollection.CopyTo(Array array, int index)
            {
                TValue[] values = this.ToArray();
                values.CopyTo(array, index);
            }
        }

        private class DictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> _enumerator;

            public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
            {
                _enumerator = enumerator;
            }

            public object Current
            {
                get
                {
                    return _enumerator.Current;
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    return new DictionaryEntry(_enumerator.Current.Key, _enumerator.Current.Value);
                }
            }

            public object Key
            {
                get
                {
                    return _enumerator.Current.Key;
                }
            }

            public object Value
            {
                get
                {
                    return _enumerator.Current.Value;
                }
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}
