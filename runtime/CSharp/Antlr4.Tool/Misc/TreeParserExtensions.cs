// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Misc
{
    using System.Text.RegularExpressions;
    using Antlr.Runtime.Tree;
    using ArgumentException = System.ArgumentException;
    using StringSplitOptions = System.StringSplitOptions;

    internal static class TreeParserExtensions
    {
        private const string DotDot = ".*[^.]\\.\\.[^.].*";
        private const string DoubleEtc = ".*\\.\\.\\.\\s+\\.\\.\\..*";


        /** Check if current node in input has a context.  Context means sequence
         *  of nodes towards root of tree.  For example, you might say context
         *  is "MULT" which means my parent must be MULT.  "CLASS VARDEF" says
         *  current node must be child of a VARDEF and whose parent is a CLASS node.
         *  You can use "..." to mean zero-or-more nodes.  "METHOD ... VARDEF"
         *  means my parent is VARDEF and somewhere above that is a METHOD node.
         *  The first node in the context is not necessarily the root.  The context
         *  matcher stops matching and returns true when it runs out of context.
         *  There is no way to force the first node to be the root.
         */
        public static bool InContext(this TreeParser parser, string context)
        {
            return InContext(parser.GetTreeNodeStream().TreeAdaptor, parser.TokenNames, parser.GetTreeNodeStream().LT(1), context);
        }

        /// <summary>
        /// The worker for <see cref="InContext(TreeParser, string)"/>. It's <see langword="static"/> and full of
        /// parameters for testing purposes.
        /// </summary>
        private static bool InContext(
            ITreeAdaptor adaptor,
            string[] tokenNames,
            object t,
            string context)
        {
            if (Regex.IsMatch(context, DotDot))
            {
                // don't allow "..", must be "..."
                throw new ArgumentException("invalid syntax: ..");
            }

            if (Regex.IsMatch(context, DoubleEtc))
            {
                // don't allow double "..."
                throw new ArgumentException("invalid syntax: ... ...");
            }

            context = context.Replace("...", " ... "); // ensure spaces around ...
            context = context.Trim();
            string[] nodes = context.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int ni = nodes.Length - 1;
            t = adaptor.GetParent(t);
            while (ni >= 0 && t != null)
            {
                if (nodes[ni].Equals("..."))
                {
                    // walk upwards until we see nodes[ni-1] then continue walking
                    if (ni == 0)
                    {
                        // ... at start is no-op
                        return true;
                    }

                    string goal = nodes[ni - 1];
                    object ancestor = GetAncestor(adaptor, tokenNames, t, goal);
                    if (ancestor == null)
                        return false;

                    t = ancestor;
                    ni--;
                }

                string name = tokenNames[adaptor.GetType(t)];
                if (!name.Equals(nodes[ni]))
                {
                    //System.Console.Error.WriteLine("not matched: " + nodes[ni] + " at " + t);
                    return false;
                }

                // advance to parent and to previous element in context node list
                ni--;
                t = adaptor.GetParent(t);
            }

            if (t == null && ni >= 0)
                return false; // at root but more nodes to match
            return true;
        }

        /// <summary>
        /// Helper for static <see cref="InContext(ITreeAdaptor, string[], object, string)"/>.
        /// </summary>
        private static object GetAncestor(ITreeAdaptor adaptor, string[] tokenNames, object t, string goal)
        {
            while (t != null)
            {
                string name = tokenNames[adaptor.GetType(t)];
                if (name.Equals(goal))
                    return t;

                t = adaptor.GetParent(t);
            }

            return null;
        }
    }
}
