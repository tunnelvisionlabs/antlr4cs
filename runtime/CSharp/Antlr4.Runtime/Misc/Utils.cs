// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* Copyright (c) 2012 The ANTLR Project. All rights reserved.
* Use of this file is governed by the BSD-3-Clause license that
* can be found in the LICENSE.txt file in the project root.
*/
using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Misc
{
    public class Utils
    {
        public static int NumNonnull(object[] data)
        {
            int n = 0;
            if (data == null)
            {
                return n;
            }
            foreach (object o in data)
            {
                if (o != null)
                {
                    n++;
                }
            }
            return n;
        }

        public static void RemoveAllElements<T>(ICollection<T> data, T value)
        {
            if (data == null)
            {
                return;
            }
            while (data.Contains(value))
            {
                data.Remove(value);
            }
        }

        public static string EscapeWhitespace(string s, bool escapeSpaces)
        {
            StringBuilder buf = new StringBuilder();
            foreach (char c in s.ToCharArray())
            {
                if (c == ' ' && escapeSpaces)
                {
                    buf.Append('\u00B7');
                }
                else
                {
                    if (c == '\t')
                    {
                        buf.Append("\\t");
                    }
                    else
                    {
                        if (c == '\n')
                        {
                            buf.Append("\\n");
                        }
                        else
                        {
                            if (c == '\r')
                            {
                                buf.Append("\\r");
                            }
                            else
                            {
                                buf.Append(c);
                            }
                        }
                    }
                }
            }
            return buf.ToString();
        }

        public static void RemoveAll<T, _T1>(IList<T> list, Predicate<_T1> predicate)
        {
            int j = 0;
            for (int i = 0; i < list.Count; i++)
            {
                T item = list[i];
                if (!predicate.Eval(item))
                {
                    if (j != i)
                    {
                        list.Set(j, item);
                    }
                    j++;
                }
            }
            if (j < list.Count)
            {
                list.SubList(j, list.Count).Clear();
            }
        }

        /// <summary>Convert array of strings to string&rarr;index map.</summary>
        /// <remarks>
        /// Convert array of strings to string&rarr;index map. Useful for
        /// converting rulenames to name&rarr;ruleindex map.
        /// </remarks>
        public static IDictionary<string, int> ToMap(string[] keys)
        {
            IDictionary<string, int> m = new Dictionary<string, int>();
            for (int i = 0; i < keys.Length; i++)
            {
                m[keys[i]] = i;
            }
            return m;
        }

        public static char[] ToCharArray(List<int> data)
        {
            if (data == null)
            {
                return null;
            }
            char[] cdata = new char[data.Size()];
            for (int i = 0; i < data.Size(); i++)
            {
                cdata[i] = (char)data.Get(i);
            }
            return cdata;
        }

        /// <since>4.5</since>
        [NotNull]
        public static IntervalSet ToSet(BitSet bits)
        {
            IntervalSet s = new IntervalSet();
            int i = bits.NextSetBit(0);
            while (i >= 0)
            {
                s.Add(i);
                i = bits.NextSetBit(i + 1);
            }
            return s;
        }
    }
}
