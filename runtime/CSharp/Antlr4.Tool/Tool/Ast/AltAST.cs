// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool.Ast
{
    using Antlr.Runtime;
    using Antlr4.Analysis;
    using ITree = Antlr.Runtime.Tree.ITree;

    /** Any ALT (which can be child of ALT_REWRITE node) */
    public class AltAST : GrammarASTWithOptions
    {
        public Alternative alt;

        /** If we transformed this alt from a left-recursive one, need info on it */
        public LeftRecursiveRuleAltInfo leftRecursiveAltInfo;

        /** If someone specified an outermost alternative label with #foo.
         *  Token type will be ID.
         */
        public GrammarAST altLabel;

        public AltAST(AltAST node)
            : base(node)
        {
            this.alt = node.alt;
            this.altLabel = node.altLabel;
            this.leftRecursiveAltInfo = node.leftRecursiveAltInfo;
        }

        public AltAST(IToken t)
            : base(t)
        {
        }

        public AltAST(int type)
            : base(type)
        {
        }

        public AltAST(int type, IToken t)
            : base(type, t)
        {
        }

        public AltAST(int type, IToken t, string text)
            : base(type, t, text)
        {
        }

        public override ITree DupNode()
        {
            return new AltAST(this);
        }

        public override object Visit(GrammarASTVisitor v)
        {
            return v.Visit(this);
        }
    }
}
