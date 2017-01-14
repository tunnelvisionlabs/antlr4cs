// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool.Ast
{
    using IToken = Antlr.Runtime.IToken;
    using ITree = Antlr.Runtime.Tree.ITree;

    public class OptionalBlockAST : GrammarAST, RuleElementAST, QuantifierAST
    {
        private readonly bool _greedy;

        public OptionalBlockAST(OptionalBlockAST node)
            : base(node)
        {
            _greedy = node._greedy;
        }

        public OptionalBlockAST(int type, IToken t, IToken nongreedy)
            : base(type, t)
        {
            _greedy = nongreedy == null;
        }

        public virtual bool GetGreedy()
        {
            return _greedy;
        }

        public override ITree DupNode()
        {
            return new OptionalBlockAST(this);
        }

        public override object Visit(GrammarASTVisitor v)
        {
            return v.Visit(this);
        }
    }
}
