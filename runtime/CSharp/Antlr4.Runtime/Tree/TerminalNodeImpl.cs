// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* Copyright (c) 2012 The ANTLR Project. All rights reserved.
* Use of this file is governed by the BSD-3-Clause license that
* can be found in the LICENSE.txt file in the project root.
*/
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree
{
    public class TerminalNodeImpl : ITerminalNode
    {
        public IToken symbol;

        public IRuleNode parent;

        public TerminalNodeImpl(IToken symbol)
        {
            this.symbol = symbol;
        }

        public virtual IParseTree GetChild(int i)
        {
            return null;
        }

        public virtual IToken Symbol
        {
            get
            {
                return symbol;
            }
        }

        public virtual IRuleNode Parent
        {
            get
            {
                return parent;
            }
        }

        public virtual IToken Payload
        {
            get
            {
                return symbol;
            }
        }

        public virtual Interval SourceInterval
        {
            get
            {
                if (symbol != null)
                {
                    int tokenIndex = symbol.TokenIndex;
                    return new Interval(tokenIndex, tokenIndex);
                }
                return Interval.Invalid;
            }
        }

        public virtual int ChildCount
        {
            get
            {
                return 0;
            }
        }

        public virtual T Accept<T, _T1>(IParseTreeVisitor<_T1> visitor)
            where _T1 : T
        {
            return visitor.VisitTerminal(this);
        }

        public virtual string GetText()
        {
            if (symbol != null)
            {
                return symbol.Text;
            }
            return null;
        }

        public virtual string ToStringTree(Parser parser)
        {
            return ToString();
        }

        public override string ToString()
        {
            if (symbol != null)
            {
                if (symbol.Type == TokenConstants.Eof)
                {
                    return "<EOF>";
                }
                return symbol.Text;
            }
            else
            {
                return "<null>";
            }
        }

        public virtual string ToStringTree()
        {
            return ToString();
        }
    }
}
