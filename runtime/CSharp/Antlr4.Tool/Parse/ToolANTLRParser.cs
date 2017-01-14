// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

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
