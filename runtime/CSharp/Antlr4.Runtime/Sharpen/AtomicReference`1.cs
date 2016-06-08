// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Runtime.Sharpen
{
    using Interlocked = System.Threading.Interlocked;

    public class AtomicReference<T>
        where T : class
    {
#pragma warning disable 0420 // 'fieldname': a reference to a volatile field will not be treated as volatile
        private volatile T _value;

        public AtomicReference()
        {
        }

        public AtomicReference(T value)
        {
            _value = value;
        }

        public T Get()
        {
            return _value;
        }

        public void Set(T value)
        {
            _value = value;
        }

        public bool CompareAndSet(T expect, T update)
        {
            return Interlocked.CompareExchange(ref _value, update, expect) == expect;
        }

        public T GetAndSet(T value)
        {
            return Interlocked.Exchange(ref _value, value);
        }
    }
}
