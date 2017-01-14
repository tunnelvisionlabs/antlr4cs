// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool.Ast
{
    using System.Collections.Generic;
    using Antlr.Runtime;
    using ITree = Antlr.Runtime.Tree.ITree;

    public class ActionAST : GrammarASTWithOptions, RuleElementAST
    {
        // Alt, rule, grammar space
        public AttributeResolver resolver;
        public IList<IToken> chunks; // useful for ANTLR IDE developers

        public ActionAST(ActionAST node)
            : base(node)
        {
            this.resolver = node.resolver;
            this.chunks = node.chunks;
        }

        public ActionAST(IToken t)
            : base(t)
        {
        }

        public ActionAST(int type)
            : base(type)
        {
        }

        public ActionAST(int type, IToken t)
            : base(type, t)
        {
        }

        public override ITree DupNode()
        {
            return new ActionAST(this);
        }

        public override object Visit(GrammarASTVisitor v)
        {
            return v.Visit(this);
        }
    }
}
