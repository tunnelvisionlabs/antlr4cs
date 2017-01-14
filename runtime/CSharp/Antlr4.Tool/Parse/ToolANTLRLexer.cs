// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Parse
{
    using Antlr4.Tool;
    using ICharStream = Antlr.Runtime.ICharStream;
    using IToken = Antlr.Runtime.IToken;
    using RecognitionException = Antlr.Runtime.RecognitionException;

    public class ToolANTLRLexer : ANTLRLexer
    {
        public AntlrTool tool;

        public ToolANTLRLexer(ICharStream input, AntlrTool tool)
            : base(input)
        {
            this.tool = tool;
        }

        public override void DisplayRecognitionError(string[] tokenNames, RecognitionException e)
        {
            string msg = GetErrorMessage(e, tokenNames);
            tool.errMgr.SyntaxError(ErrorType.SYNTAX_ERROR, SourceName, e.Token, e, msg);
        }

        public override void GrammarError(ErrorType etype, IToken token, params object[] args)
        {
            tool.errMgr.GrammarError(etype, SourceName, token, args);
        }
    }
}
