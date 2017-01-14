// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool.Ast
{
    using System.Collections.Generic;
    using Antlr.Runtime;
    using ITree = Antlr.Runtime.Tree.ITree;

    public class BlockAST : GrammarASTWithOptions, RuleElementAST
    {
        // TODO: maybe I need a Subrule object like Rule so these options mov to that?
        /** What are the default options for a subrule? */
        public static readonly IDictionary<string, string> defaultBlockOptions =
            new Dictionary<string, string>();

        public static readonly IDictionary<string, string> defaultLexerBlockOptions =
            new Dictionary<string, string>();

        public BlockAST(BlockAST node)
            : base(node)
        {
        }

        public BlockAST(IToken t)
            : base(t)
        {
        }

        public BlockAST(int type)
            : base(type)
        {
        }

        public BlockAST(int type, IToken t)
            : base(type, t)
        {
        }

        public BlockAST(int type, IToken t, string text)
            : base(type, t, text)
        {
        }

        public override ITree DupNode()
        {
            return new BlockAST(this);
        }

        public override object Visit(GrammarASTVisitor v)
        {
            return v.Visit(this);
        }
    }
}
