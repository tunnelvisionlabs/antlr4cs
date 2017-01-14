// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
