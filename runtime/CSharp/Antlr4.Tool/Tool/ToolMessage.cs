// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

namespace Antlr4.Tool
{
    using CommonToken = Antlr.Runtime.CommonToken;
    using Exception = System.Exception;
    using TokenTypes = Antlr.Runtime.TokenTypes;

    /** A generic message from the tool such as "file not found" type errors; there
     *  is no reason to create a special object for each error unlike the grammar
     *  errors, which may be rather complex.
     *
     *  Sometimes you need to pass in a filename or something to say it is "bad".
     *  Allow a generic object to be passed in and the string template can deal
     *  with just printing it or pulling a property out of it.
     */
    public class ToolMessage : ANTLRMessage
    {
        public ToolMessage(ErrorType errorType)
            : base(errorType)
        {
        }

        public ToolMessage(ErrorType errorType, params object[] args)
            : base(errorType, null, new CommonToken(TokenTypes.Invalid), args)
        {
        }

        public ToolMessage(ErrorType errorType, Exception e, params object[] args)
            : base(errorType, e, new CommonToken(TokenTypes.Invalid), args)
        {
        }
    }
}
