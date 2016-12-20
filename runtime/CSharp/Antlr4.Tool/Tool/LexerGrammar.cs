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
