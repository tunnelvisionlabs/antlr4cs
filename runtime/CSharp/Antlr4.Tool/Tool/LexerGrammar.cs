// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    using System.Diagnostics;
    using Antlr4.Tool.Ast;

    /** */
    public class LexerGrammar : Grammar
    {
        public static readonly string DEFAULT_MODE_NAME = "DEFAULT_MODE";

        /** The grammar from which this lexer grammar was derived (if implicit) */
        public Grammar implicitLexerOwner;

        /** DEFAULT_MODE rules are added first due to grammar syntax order */
        public Runtime.Misc.MultiMap<string, Rule> modes;

        public LexerGrammar(AntlrTool tool, GrammarRootAST ast)
            : base(tool, ast)
        {
        }

        public LexerGrammar(string grammarText)
            : base(grammarText)
        {
        }

        public LexerGrammar(string grammarText, ANTLRToolListener listener)
            : base(grammarText, listener)
        {
        }

        public LexerGrammar(string fileName, string grammarText, ANTLRToolListener listener)
            : base(fileName, grammarText, listener)
        {
        }

        public override bool DefineRule(Rule r)
        {
            if (!base.DefineRule(r))
            {
                return false;
            }

            if (modes == null)
                modes = new Runtime.Misc.MultiMap<string, Rule>();
            modes.Map(r.mode, r);
            return true;
        }

        public override bool UndefineRule(Rule r)
        {
            if (!base.UndefineRule(r))
            {
                return false;
            }

            bool removed = modes[r.mode].Remove(r);
            Debug.Assert(removed);
            return true;
        }
    }
}
