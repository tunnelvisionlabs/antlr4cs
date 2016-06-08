// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Tree.Xpath;

namespace Antlr4.Runtime.Tree.Xpath
{
    public class XPathLexerErrorListener : IAntlrErrorListener<int>
    {
        public virtual void SyntaxError<T>(Recognizer<T, object> recognizer, T offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            where T : int
        {
        }
    }
}
