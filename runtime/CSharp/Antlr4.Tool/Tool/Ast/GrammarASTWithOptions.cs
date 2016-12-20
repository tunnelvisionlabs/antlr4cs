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
    using Antlr4.Misc;
    using IToken = Antlr.Runtime.IToken;
    using ITree = Antlr.Runtime.Tree.ITree;
    using NotNullAttribute = Antlr4.Runtime.Misc.NotNullAttribute;

    public abstract class GrammarASTWithOptions : GrammarAST
    {
        protected IDictionary<string, GrammarAST> options;

        public GrammarASTWithOptions(GrammarASTWithOptions node)
            : base(node)
        {
            this.options = node.options;
        }

        public GrammarASTWithOptions(IToken t)
            : base(t)
        {
        }

        public GrammarASTWithOptions(int type)
            : base(type)
        {
        }

        public GrammarASTWithOptions(int type, IToken t)
            : base(type, t)
        {
        }

        public GrammarASTWithOptions(int type, IToken t, string text)
            : base(type, t, text)
        {
        }

        public virtual void SetOption(string key, GrammarAST node)
        {
            if (options == null)
                options = new Dictionary<string, GrammarAST>();
            options[key] = node;
        }

        public virtual string GetOptionString(string key)
        {
            GrammarAST value = GetOptionAST(key);
            if (value == null)
                return null;
            if (value is ActionAST)
            {
                return value.Text;
            }
            else
            {
                string v = value.Text;
                if (v.StartsWith("'") || v.StartsWith("\""))
                {
                    v = CharSupport.GetStringFromGrammarStringLiteral(v);
                }
                return v;
            }
        }

        /** Gets AST node holding value for option key; ignores default options
         *  and command-line forced options.
         */
        public virtual GrammarAST GetOptionAST(string key)
        {
            if (options == null)
                return null;

            GrammarAST value;
            if (!options.TryGetValue(key, out value))
                return null;

            return value;
        }

        public virtual int GetNumberOfOptions()
        {
            return options == null ? 0 : options.Count;
        }

        public override abstract ITree DupNode();

        [return: NotNull]
        public virtual IDictionary<string, GrammarAST> GetOptions()
        {
            if (options == null)
            {
                return new Dictionary<string, GrammarAST>();
            }

            return options;
        }
    }
}
