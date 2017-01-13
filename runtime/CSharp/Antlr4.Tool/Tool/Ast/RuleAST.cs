// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool.Ast
{
    using Antlr4.Parse;
    using IToken = Antlr.Runtime.IToken;
    using ITree = Antlr.Runtime.Tree.ITree;

    public class RuleAST : GrammarASTWithOptions
    {
        public RuleAST(RuleAST node)
            : base(node)
        {
        }

        public RuleAST(IToken t)
            : base(t)
        {
        }

        public RuleAST(int type)
            : base(type)
        {
        }

        public virtual bool IsLexerRule()
        {
            string name = GetRuleName();
            return name != null && Grammar.IsTokenName(name);
        }

        public virtual string GetRuleName()
        {
            GrammarAST nameNode = (GrammarAST)GetChild(0);
            if (nameNode != null)
                return nameNode.Text;
            return null;
        }

        public override ITree DupNode()
        {
            return new RuleAST(this);
        }

        public virtual ActionAST GetLexerAction()
        {
            ITree blk = GetFirstChildWithType(ANTLRParser.BLOCK);
            if (blk.ChildCount == 1)
            {
                ITree onlyAlt = blk.GetChild(0);
                ITree lastChild = onlyAlt.GetChild(onlyAlt.ChildCount - 1);
                if (lastChild.Type == ANTLRParser.ACTION)
                {
                    return (ActionAST)lastChild;
                }
            }
            return null;
        }

        public override object Visit(GrammarASTVisitor v)
        {
            return v.Visit(this);
        }
    }
}
