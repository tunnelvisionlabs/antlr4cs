// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree
{
    public class ParseTreeWalker
    {
        public static readonly ParseTreeWalker Default = new ParseTreeWalker();

        public virtual void Walk(IParseTreeListener listener, IParseTree t)
        {
            Stack<IParseTree> nodeStack = new Stack<IParseTree>();
            List<int> indexStack = new List<int>();
            IParseTree currentNode = t;
            int currentIndex = 0;
            while (currentNode != null)
            {
                // pre-order visit
                if (currentNode is IErrorNode)
                {
                    listener.VisitErrorNode((IErrorNode)currentNode);
                }
                else
                {
                    if (currentNode is ITerminalNode)
                    {
                        listener.VisitTerminal((ITerminalNode)currentNode);
                    }
                    else
                    {
                        IRuleNode r = (IRuleNode)currentNode;
                        EnterRule(listener, r);
                    }
                }
                // Move down to first child, if exists
                if (currentNode.ChildCount > 0)
                {
                    nodeStack.Push(currentNode);
                    indexStack.Add(currentIndex);
                    currentIndex = 0;
                    currentNode = currentNode.GetChild(0);
                    continue;
                }
                do
                {
                    // No child nodes, so walk tree
                    // post-order visit
                    if (currentNode is IRuleNode)
                    {
                        ExitRule(listener, (IRuleNode)currentNode);
                    }
                    // No parent, so no siblings
                    if (nodeStack.IsEmpty())
                    {
                        currentNode = null;
                        currentIndex = 0;
                        break;
                    }
                    // Move to next sibling if possible
                    currentNode = nodeStack.Peek().GetChild(++currentIndex);
                    if (currentNode != null)
                    {
                        break;
                    }
                    // No next sibling, so move up
                    currentNode = nodeStack.Pop();
                    currentIndex = indexStack.Pop();
                }
                while (currentNode != null);
            }
        }

        /// <summary>
        /// The discovery of a rule node, involves sending two events: the generic
        /// <see cref="IParseTreeListener.EnterEveryRule(Antlr4.Runtime.ParserRuleContext)"/>
        /// and a
        /// <see cref="Antlr4.Runtime.RuleContext"/>
        /// -specific event. First we trigger the generic and then
        /// the rule specific. We to them in reverse order upon finishing the node.
        /// </summary>
        protected internal virtual void EnterRule(IParseTreeListener listener, IRuleNode r)
        {
            ParserRuleContext ctx = (ParserRuleContext)r.RuleContext;
            listener.EnterEveryRule(ctx);
            ctx.EnterRule(listener);
        }

        protected internal virtual void ExitRule(IParseTreeListener listener, IRuleNode r)
        {
            ParserRuleContext ctx = (ParserRuleContext)r.RuleContext;
            ctx.ExitRule(listener);
            listener.ExitEveryRule(ctx);
        }
    }
}
