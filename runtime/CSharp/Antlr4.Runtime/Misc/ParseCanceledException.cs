// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System;
using Antlr4.Runtime.Sharpen;

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
        private const long serialVersionUID = -3529552099366979683L;

        public ParseCanceledException()
        {
        }

        public ParseCanceledException(string message)
            : base(message)
        {
        }

        public ParseCanceledException(Exception cause)
        {
            Antlr4.Runtime.Sharpen.Extensions.InitCause(this, cause);
        }

        public ParseCanceledException(string message, Exception cause)
            : base(message)
        {
            Antlr4.Runtime.Sharpen.Extensions.InitCause(this, cause);
        }
    }
}
