// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool.Ast
{
    using IToken = Antlr.Runtime.IToken;
    using ITree = Antlr.Runtime.Tree.ITree;

    public class TerminalAST : GrammarASTWithOptions, RuleElementAST
    {
        public TerminalAST(TerminalAST node)
            : base(node)
        {
        }

        public TerminalAST(IToken t)
            : base(t)
        {
        }

        public TerminalAST(int type)
            : base(type)
        {
        }

        public TerminalAST(int type, IToken t)
            : base(type, t)
        {
        }

        public override ITree DupNode()
        {
            return new TerminalAST(this);
        }

        public override object Visit(GrammarASTVisitor v)
        {
            return v.Visit(this);
        }
    }
}
