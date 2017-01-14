// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Parse
{
    using IIntStream = Antlr.Runtime.IIntStream;
    using RecognitionException = Antlr.Runtime.RecognitionException;

    /** */
    public class v4ParserException : RecognitionException
    {
        public string msg;

        public v4ParserException(string msg, IIntStream input)
            : base(input)
        {
            this.msg = msg;
        }
    }
}
