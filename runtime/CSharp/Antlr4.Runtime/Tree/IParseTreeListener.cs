// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree
{
    /// <summary>
    /// This interface describes the minimal core of methods triggered
    /// by
    /// <see cref="ParseTreeWalker"/>
    /// . E.g.,
    /// ParseTreeWalker walker = new ParseTreeWalker();
    /// walker.walk(myParseTreeListener, myParseTree); &lt;-- triggers events in your listener
    /// If you want to trigger events in multiple listeners during a single
    /// tree walk, you can use the ParseTreeDispatcher object available at
    /// https://github.com/antlr/antlr4/issues/841
    /// </summary>
    public interface IParseTreeListener
    {
        void VisitTerminal(ITerminalNode node);

        void VisitErrorNode(IErrorNode node);

        void EnterEveryRule(ParserRuleContext ctx);

        void ExitEveryRule(ParserRuleContext ctx);
    }
}
