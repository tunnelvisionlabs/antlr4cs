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
    using ArgumentNullException = System.ArgumentNullException;
    using IToken = Antlr.Runtime.IToken;
    using ITokenStream = Antlr.Runtime.ITokenStream;
    using ITree = Antlr.Runtime.Tree.ITree;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;

    public class GrammarRootAST : GrammarASTWithOptions
    {
        public static readonly IDictionary<string, string> defaultOptions = new Dictionary<string, string>
        {
            { "language", "Java" },
            { "abstract", "false" }
        };

        public int grammarType; // LEXER, PARSER, GRAMMAR (combined)
        public bool hasErrors;
        /** Track stream used to create this tree */
        [NotNull]
        public readonly ITokenStream tokenStream;
        public IDictionary<string, string> cmdLineOptions; // -DsuperClass=T on command line
        public string fileName;

        public GrammarRootAST(GrammarRootAST node)
            : base(node)
        {
            this.grammarType = node.grammarType;
            this.hasErrors = node.hasErrors;
            this.tokenStream = node.tokenStream;
        }

        public GrammarRootAST(IToken t, ITokenStream tokenStream)
            : base(t)
        {
            if (tokenStream == null)
            {
                throw new ArgumentNullException(nameof(tokenStream));
            }

            this.tokenStream = tokenStream;
        }

        public GrammarRootAST(int type, IToken t, ITokenStream tokenStream)
            : base(type, t)
        {
            if (tokenStream == null)
            {
                throw new ArgumentNullException(nameof(tokenStream));
            }

            this.tokenStream = tokenStream;
        }

        public GrammarRootAST(int type, IToken t, string text, ITokenStream tokenStream)
            : base(type, t, text)
        {
            if (tokenStream == null)
            {
                throw new ArgumentNullException(nameof(tokenStream));
            }

            this.tokenStream = tokenStream;
        }

        public virtual string GetGrammarName()
        {
            ITree t = GetChild(0);
            if (t != null)
                return t.Text;
            return null;
        }

        public override string GetOptionString(string key)
        {
            if (cmdLineOptions != null && cmdLineOptions.ContainsKey(key))
            {
                return cmdLineOptions[key];
            }
            string value = base.GetOptionString(key);
            if (value == null)
            {
                defaultOptions.TryGetValue(key, out value);
            }
            return value;
        }

        public override object Visit(GrammarASTVisitor v)
        {
            return v.Visit(this);
        }

        public override ITree DupNode()
        {
            return new GrammarRootAST(this);
        }
    }
}
