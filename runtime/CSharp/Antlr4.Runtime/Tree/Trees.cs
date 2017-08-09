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
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree
{
    /// <summary>A set of utility routines useful for all kinds of ANTLR trees.</summary>
    public class Trees
    {
        /// <summary>Print out a whole tree in LISP form.</summary>
        /// <remarks>
        /// Print out a whole tree in LISP form.
        /// <see cref="GetNodeText(ITree, Antlr4.Runtime.Parser)"/>
        /// is used on the
        /// node payloads to get the text for the nodes.  Detect
        /// parse trees and extract data appropriately.
        /// </remarks>
        public static string ToStringTree([NotNull] ITree t)
        {
            return ToStringTree(t, (IList<string>)null);
        }

        /// <summary>Print out a whole tree in LISP form.</summary>
        /// <remarks>
        /// Print out a whole tree in LISP form.
        /// <see cref="GetNodeText(ITree, Antlr4.Runtime.Parser)"/>
        /// is used on the
        /// node payloads to get the text for the nodes.  Detect
        /// parse trees and extract data appropriately.
        /// </remarks>
        public static string ToStringTree([NotNull] ITree t, [Nullable] Parser recog)
        {
            string[] ruleNames = recog != null ? recog.RuleNames : null;
            IList<string> ruleNamesList = ruleNames != null ? Arrays.AsList(ruleNames) : null;
            return ToStringTree(t, ruleNamesList);
        }

        /// <summary>Print out a whole tree in LISP form.</summary>
        /// <remarks>
        /// Print out a whole tree in LISP form.
        /// <see cref="GetNodeText(ITree, Antlr4.Runtime.Parser)"/>
        /// is used on the
        /// node payloads to get the text for the nodes.
        /// </remarks>
        public static string ToStringTree([NotNull] ITree t, [Nullable] IList<string> ruleNames)
        {
            string s = Utils.EscapeWhitespace(GetNodeText(t, ruleNames), false);
            if (t.ChildCount == 0)
            {
                return s;
            }
            StringBuilder buf = new StringBuilder();
            buf.Append("(");
            s = Utils.EscapeWhitespace(GetNodeText(t, ruleNames), false);
            buf.Append(s);
            buf.Append(' ');
            for (int i = 0; i < t.ChildCount; i++)
            {
                if (i > 0)
                {
                    buf.Append(' ');
                }
                buf.Append(ToStringTree(t.GetChild(i), ruleNames));
            }
            buf.Append(")");
            return buf.ToString();
        }

        public static string GetNodeText([NotNull] ITree t, [Nullable] Parser recog)
        {
            string[] ruleNames = recog != null ? recog.RuleNames : null;
            IList<string> ruleNamesList = ruleNames != null ? Arrays.AsList(ruleNames) : null;
            return GetNodeText(t, ruleNamesList);
        }

        public static string GetNodeText([NotNull] ITree t, [Nullable] IList<string> ruleNames)
        {
            if (ruleNames != null)
            {
                if (t is IRuleNode)
                {
                    RuleContext ruleContext = ((IRuleNode)t).RuleContext;
                    int ruleIndex = ruleContext.RuleIndex;
                    string ruleName = ruleNames[ruleIndex];
                    int altNumber = ruleContext.OuterAlternative;
                    if (altNumber != ATN.InvalidAltNumber)
                    {
                        return ruleName + ":" + altNumber;
                    }
                    return ruleName;
                }
                else
                {
                    if (t is IErrorNode)
                    {
                        return t.ToString();
                    }
                    else
                    {
                        if (t is ITerminalNode)
                        {
                            IToken symbol = ((ITerminalNode)t).Symbol;
                            if (symbol != null)
                            {
                                string s = symbol.Text;
                                return s;
                            }
                        }
                    }
                }
            }
            // no recog for rule names
            object payload = t.Payload;
            if (payload is IToken)
            {
                return ((IToken)payload).Text;
            }
            return t.Payload.ToString();
        }

        /// <summary>Return ordered list of all children of this node</summary>
        public static IList<ITree> GetChildren(ITree t)
        {
            IList<ITree> kids = new List<ITree>();
            for (int i = 0; i < t.ChildCount; i++)
            {
                kids.Add(t.GetChild(i));
            }
            return kids;
        }

        /// <summary>Return a list of all ancestors of this node.</summary>
        /// <remarks>
        /// Return a list of all ancestors of this node.  The first node of
        /// list is the root and the last is the parent of this node.
        /// </remarks>
        /// <since>4.5.1</since>
        [NotNull]
        public static IList<ITree> GetAncestors([NotNull] ITree t)
        {
            if (t.Parent == null)
            {
                return Antlr4.Runtime.Sharpen.Collections.EmptyList();
            }
            IList<ITree> ancestors = new List<ITree>();
            t = t.Parent;
            while (t != null)
            {
                ancestors.Add(0, t);
                // insert at start
                t = t.Parent;
            }
            return ancestors;
        }

        /// <summary>Return true if t is u's parent or a node on path to root from u.</summary>
        /// <remarks>
        /// Return true if t is u's parent or a node on path to root from u.
        /// Use == not equals().
        /// </remarks>
        /// <since>4.5.1</since>
        public static bool IsAncestorOf(ITree t, ITree u)
        {
            if (t == null || u == null || t.Parent == null)
            {
                return false;
            }
            ITree p = u.Parent;
            while (p != null)
            {
                if (t == p)
                {
                    return true;
                }
                p = p.Parent;
            }
            return false;
        }

        public static ICollection<IParseTree> FindAllTokenNodes(IParseTree t, int ttype)
        {
            return FindAllNodes(t, ttype, true);
        }

        public static ICollection<IParseTree> FindAllRuleNodes(IParseTree t, int ruleIndex)
        {
            return FindAllNodes(t, ruleIndex, false);
        }

        public static IList<IParseTree> FindAllNodes(IParseTree t, int index, bool findTokens)
        {
            IList<IParseTree> nodes = new List<IParseTree>();
            _findAllNodes(t, index, findTokens, nodes);
            return nodes;
        }

        public static void _findAllNodes<_T0>(IParseTree t, int index, bool findTokens, IList<_T0> nodes)
        {
            // check this node (the root) first
            if (findTokens && t is ITerminalNode)
            {
                ITerminalNode tnode = (ITerminalNode)t;
                if (tnode.Symbol.Type == index)
                {
                    nodes.Add(t);
                }
            }
            else
            {
                if (!findTokens && t is ParserRuleContext)
                {
                    ParserRuleContext ctx = (ParserRuleContext)t;
                    if (ctx.RuleIndex == index)
                    {
                        nodes.Add(t);
                    }
                }
            }
            // check children
            for (int i = 0; i < t.ChildCount; i++)
            {
                _findAllNodes(t.GetChild(i), index, findTokens, nodes);
            }
        }

        /// <summary>Get all descendents; includes t itself.</summary>
        /// <since>4.5.1</since>
        public static IList<IParseTree> GetDescendants(IParseTree t)
        {
            IList<IParseTree> nodes = new List<IParseTree>();
            nodes.Add(t);
            int n = t.ChildCount;
            for (int i = 0; i < n; i++)
            {
                Sharpen.Collections.AddAll(nodes, GetDescendants(t.GetChild(i)));
            }
            return nodes;
        }

        [System.ObsoleteAttribute(@"")]
        public static IList<IParseTree> Descendants(IParseTree t)
        {
            return GetDescendants(t);
        }

        /// <summary>
        /// Find smallest subtree of t enclosing range startTokenIndex..stopTokenIndex
        /// inclusively using postorder traversal.
        /// </summary>
        /// <remarks>
        /// Find smallest subtree of t enclosing range startTokenIndex..stopTokenIndex
        /// inclusively using postorder traversal.  Recursive depth-first-search.
        /// </remarks>
        /// <since>4.5</since>
        [Nullable]
        public static ParserRuleContext GetRootOfSubtreeEnclosingRegion([NotNull] IParseTree t, int startTokenIndex, int stopTokenIndex)
        {
            // inclusive
            // inclusive
            int n = t.ChildCount;
            for (int i = 0; i < n; i++)
            {
                IParseTree child = t.GetChild(i);
                ParserRuleContext r = GetRootOfSubtreeEnclosingRegion(child, startTokenIndex, stopTokenIndex);
                if (r != null)
                {
                    return r;
                }
            }
            if (t is ParserRuleContext)
            {
                ParserRuleContext r = (ParserRuleContext)t;
                if (startTokenIndex >= r.Start.TokenIndex && (r.Stop == null || stopTokenIndex <= r.Stop.TokenIndex))
                {
                    // is range fully contained in t?
                    // note: r.getStop()==null likely implies that we bailed out of parser and there's nothing to the right
                    return r;
                }
            }
            return null;
        }

        /// <summary>
        /// Replace any subtree siblings of root that are completely to left
        /// or right of lookahead range with a CommonToken(Token.INVALID_TYPE,"...")
        /// node.
        /// </summary>
        /// <remarks>
        /// Replace any subtree siblings of root that are completely to left
        /// or right of lookahead range with a CommonToken(Token.INVALID_TYPE,"...")
        /// node. The source interval for t is not altered to suit smaller range!
        /// WARNING: destructive to t.
        /// </remarks>
        /// <since>4.5.1</since>
        public static void StripChildrenOutOfRange(ParserRuleContext t, ParserRuleContext root, int startIndex, int stopIndex)
        {
            if (t == null)
            {
                return;
            }
            for (int i = 0; i < t.ChildCount; i++)
            {
                IParseTree child = t.GetChild(i);
                Interval range = child.SourceInterval;
                if (child is ParserRuleContext && (range.b < startIndex || range.a > stopIndex))
                {
                    if (IsAncestorOf(child, root))
                    {
                        // replace only if subtree doesn't have displayed root
                        CommonToken abbrev = new CommonToken(TokenConstants.InvalidType, "...");
                        t.children.Set(i, new TerminalNodeImpl(abbrev));
                    }
                }
            }
        }

        /// <summary>Return first node satisfying the pred</summary>
        /// <since>4.5.1</since>
        public static ITree FindNodeSuchThat(ITree t, Predicate<ITree> pred)
        {
            if (pred.Eval(t))
            {
                return t;
            }
            int n = t.ChildCount;
            for (int i = 0; i < n; i++)
            {
                ITree u = FindNodeSuchThat(t.GetChild(i), pred);
                if (u != null)
                {
                    return u;
                }
            }
            return null;
        }

        private Trees()
        {
        }
    }
}
