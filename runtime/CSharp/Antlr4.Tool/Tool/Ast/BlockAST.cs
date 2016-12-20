/*
 * [The "BSD license"]
 *  Copyright (c) 2012 Terence Parr
 *  Copyright (c) 2012 Sam Harwell
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *  1. Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 *  3. The name of the author may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 *  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 *  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 *  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 *  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 *  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 *  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 *  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
