// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Misc
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /** Count how many of each key we have; not thread safe */
    public class FrequencySet<T> : Dictionary<T, StrongBox<int>>
    {
        public virtual int GetCount(T key)
        {
            StrongBox<int> value;
            if (!TryGetValue(key, out value))
                return 0;

            return value.Value;
        }

        public virtual void Add(T key)
        {
            StrongBox<int> value;
            if (!TryGetValue(key, out value))
            {
                value = new StrongBox<int>(1);
                Add(key, value);
            }
            else
            {
                value.Value++;
            }
        }
    }
}
