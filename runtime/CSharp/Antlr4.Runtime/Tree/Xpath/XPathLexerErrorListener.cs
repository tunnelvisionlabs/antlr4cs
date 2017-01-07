// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
* Copyright (c) 2012 The ANTLR Project. All rights reserved.
* Use of this file is governed by the BSD-3-Clause license that
* can be found in the LICENSE.txt file in the project root.
*/
using Antlr4.Runtime;
using Antlr4.Runtime.Sharpen;

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
