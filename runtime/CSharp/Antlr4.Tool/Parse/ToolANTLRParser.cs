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

namespace Antlr4.Parse
{
    using Antlr4.Tool;
    using ITokenStream = Antlr.Runtime.ITokenStream;
    using NoViableAltException = Antlr.Runtime.NoViableAltException;
    using Parser = Antlr.Runtime.Parser;
    using RecognitionException = Antlr.Runtime.RecognitionException;

    /** Override error handling for use with ANTLR tool itself; leaves
     *  nothing in grammar associated with Tool so others can use in IDEs, ...
     */
    public class ToolANTLRParser : ANTLRParser
    {
        public AntlrTool tool;

        public ToolANTLRParser(ITokenStream input, AntlrTool tool)
            : base(input)
        {
            this.tool = tool;
        }

        public override void DisplayRecognitionError(string[] tokenNames,
                                            RecognitionException e)
        {
            string msg = GetParserErrorMessage(this, e);
            if (paraphrases.Count > 0)
            {
                string paraphrase = paraphrases.Peek();
                msg = msg + " while " + paraphrase;
            }
            //List stack = getRuleInvocationStack(e, this.getClass().getName());
            //msg += ", rule stack = " + stack;
            tool.errMgr.SyntaxError(ErrorType.SYNTAX_ERROR, SourceName, e.Token, e, msg);
        }

        public virtual string GetParserErrorMessage(Parser parser, RecognitionException e)
        {
            string msg;
            if (e is NoViableAltException)
            {
                string name = parser.GetTokenErrorDisplay(e.Token);
                msg = name + " came as a complete surprise to me";
            }
            else if (e is v4ParserException)
            {
                msg = ((v4ParserException)e).msg;
            }
            else
            {
                msg = parser.GetErrorMessage(e, parser.TokenNames);
            }
            return msg;
        }

        public override void GrammarError(ErrorType etype, Antlr.Runtime.IToken token, params object[] args)
        {
            tool.errMgr.GrammarError(etype, SourceName, token, args);
        }
    }
}
