// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool.Ast
{
    using IToken = Antlr.Runtime.IToken;
    using ITree = Antlr.Runtime.Tree.ITree;

    public class NotAST : GrammarAST, RuleElementAST
    {
        public NotAST(NotAST node)
            : base(node)
        {
        }

        public NotAST(int type, IToken t)
            : base(type, t)
        {
        }

        public override ITree DupNode()
        {
            return new NotAST(this);
        }

        public override object Visit(GrammarASTVisitor v)
        {
            return v.Visit(this);
        }
    }
}
