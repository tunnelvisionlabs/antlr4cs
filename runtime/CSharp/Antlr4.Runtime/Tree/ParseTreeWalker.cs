// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree
{
    public class ParseTreeWalker
    {
        public static readonly ParseTreeWalker Default = new ParseTreeWalker();

        public virtual void Walk(IParseTreeListener listener, IParseTree t)
        {
            if (t is IErrorNode)
            {
                listener.VisitErrorNode((IErrorNode)t);
                return;
            }
            else
            {
                if (t is ITerminalNode)
                {
                    listener.VisitTerminal((ITerminalNode)t);
                    return;
                }
            }
            IRuleNode r = (IRuleNode)t;
            EnterRule(listener, r);
            int n = r.ChildCount;
            for (int i = 0; i < n; i++)
            {
                Walk(listener, r.GetChild(i));
            }
            ExitRule(listener, r);
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
