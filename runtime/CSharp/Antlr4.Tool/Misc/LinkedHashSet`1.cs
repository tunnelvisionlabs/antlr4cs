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

    public class LinkedHashSet<T> : ISet<T>
    {
        private readonly Dictionary<T, LinkedListNode<T>> _dictionary;
        private readonly LinkedList<T> _list;

        public LinkedHashSet()
        {
            _dictionary = new Dictionary<T, LinkedListNode<T>>();
            _list = new LinkedList<T>();
        }

        public virtual int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public virtual bool Add(T item)
        {
            if (_dictionary.ContainsKey(item))
                return false;

            var node = new LinkedListNode<T>(item);
            _dictionary.Add(item, node);
            _list.AddLast(node);
            return true;
        }

        public virtual void Clear()
        {
            _dictionary.Clear();
            _list.Clear();
        }

        public virtual bool Contains(T item)
        {
            return _dictionary.ContainsKey(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public virtual void ExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public virtual void IntersectWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public virtual bool Overlaps(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public virtual bool Remove(T item)
        {
            LinkedListNode<T> node;
            if (!_dictionary.TryGetValue(item, out node))
                return false;

            _dictionary.Remove(item);
            _list.Remove(node);
            return true;
        }

        public virtual bool SetEquals(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public virtual void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public virtual void UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
