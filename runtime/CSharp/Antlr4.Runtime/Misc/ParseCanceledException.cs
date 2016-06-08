// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System;
using Antlr4.Runtime.Sharpen;

#if COMPACT
using OperationCanceledException = System.Exception;
#endif

namespace Antlr4.Runtime.Misc
{
    /// <summary>This exception is thrown to cancel a parsing operation.</summary>
    /// <remarks>
    /// This exception is thrown to cancel a parsing operation. This exception does
    /// not extend
    /// <see cref="Antlr4.Runtime.RecognitionException"/>
    /// , allowing it to bypass the standard
    /// error recovery mechanisms.
    /// <see cref="Antlr4.Runtime.BailErrorStrategy"/>
    /// throws this exception in
    /// response to a parse error.
    /// </remarks>
    /// <author>Sam Harwell</author>
    [System.Serializable]
    public class ParseCanceledException : OperationCanceledException
    {
        public ParseCanceledException()
        {
        }

        public ParseCanceledException(string message)
            : base(message)
        {
        }

        public ParseCanceledException(Exception cause)
            : base("The parse operation was cancelled.", cause)
        {
        }

        public ParseCanceledException(string message, Exception cause)
            : base(message, cause)
        {
        }
    }
}
