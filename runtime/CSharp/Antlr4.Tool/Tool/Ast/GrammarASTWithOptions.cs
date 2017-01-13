// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
