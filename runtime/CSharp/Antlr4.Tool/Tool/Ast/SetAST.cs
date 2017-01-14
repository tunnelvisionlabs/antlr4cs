// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool.Ast
{
    using IToken = Antlr.Runtime.IToken;
    using ITree = Antlr.Runtime.Tree.ITree;

    public class SetAST : GrammarAST, RuleElementAST
    {
        public SetAST(SetAST node)
            : base(node)
        {
        }

        public SetAST(int type, IToken t, string text)
            : base(type, t, text)
        {
        }

        public override ITree DupNode()
        {
            return new SetAST(this);
        }

        public override object Visit(GrammarASTVisitor v)
        {
            return v.Visit(this);
        }
    }
}
