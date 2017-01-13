// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Misc
{
    using System.Collections.Generic;
    using System.Text;
    using Antlr4.Tool.Ast;
    using IEnumerable = System.Collections.IEnumerable;

    /** */
    public static class Utils
    {
        public static readonly int INTEGER_POOL_MAX_VALUE = 1000;

        static object[] ints = new object[INTEGER_POOL_MAX_VALUE + 1];

        public static string StripFileExtension(string name)
        {
            if (name == null)
                return null;
            int lastDot = name.LastIndexOf('.');
            if (lastDot < 0)
                return name;
            return name.Substring(0, lastDot);
        }

        public static string Join(object[] a, string separator)
        {
            StringBuilder buf = new StringBuilder();
            for (int i = 0; i < a.Length; i++)
            {
                object o = a[i];
                buf.Append(o.ToString());
                if ((i + 1) < a.Length)
                {
                    buf.Append(separator);
                }
            }
            return buf.ToString();
        }

        public static string SortLinesInString(string s)
        {
            string[] lines = s.Split('\n');
            List<string> linesList = new List<string>(lines);
            linesList.Sort();
            linesList.CopyTo(lines);
            StringBuilder buf = new StringBuilder();
            foreach (string l in lines)
            {
                buf.Append(l);
                buf.Append('\n');
            }

            return buf.ToString();
        }

        public static IList<string> NodesToStrings<T>(IList<T> nodes)
                where T : GrammarAST
        {
            if (nodes == null)
                return null;
            IList<string> a = new List<string>();
            foreach (T t in nodes)
                a.Add(t.Text);
            return a;
        }

        //	public static <T> List<T> list(T... values) {
        //		List<T> x = new ArrayList<T>(values.length);
        //		for (T v : values) {
        //			if ( v!=null ) x.add(v);
        //		}
        //		return x;
        //	}

        public static string Capitalize(string s)
        {
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static string Decapitalize(string s)
        {
            return char.ToLower(s[0]) + s.Substring(1);
        }

        /** apply methodName to list and return list of results. method has
         *  no args.  This pulls data out of a list essentially.
         */
        public static IList<To> Select<From, To>(IList<From> list, System.Func<From, To> selector)
        {
            if (list == null)
                return null;
            IList<To> b = new List<To>();
            foreach (From f in list)
            {
                b.Add(selector(f));
            }
            return b;
        }

        /** Find exact object type or sublass of cl in list */
        public static T Find<T>(IEnumerable ops)
        {
            foreach (object o in ops)
            {
                if (o is T)
                    return (T)o;
                //			if ( o.getClass() == cl ) return o;
            }
            return default(T);
        }

        public static int IndexOf<T>(IList<T> elems, System.Predicate<T> match)
        {
            for (int i = 0; i < elems.Count; i++)
            {
                if (match(elems[i]))
                    return i;
            }
            return -1;
        }

        public static int LastIndexOf<T>(IList<T> elems, System.Predicate<T> match)
        {
            for (int i = elems.Count - 1; i >= 0; i--)
            {
                if (match(elems[i]))
                    return i;
            }
            return -1;
        }

        public static void SetSize<T>(IList<T> list, int size)
        {
            if (size < list.Count)
            {
                while (size > list.Count)
                    list.RemoveAt(list.Count - 1);
            }
            else
            {
                while (size > list.Count)
                {
                    list.Add(default(T));
                }
            }
        }
    }
}
