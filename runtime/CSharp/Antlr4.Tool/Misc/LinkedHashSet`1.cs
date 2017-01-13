// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
            foreach (T item in other)
                Add(item);
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
