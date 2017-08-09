// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    /// <summary>
    /// This signifies any kind of mismatched input exceptions such as
    /// when the current input does not match the expected token.
    /// </summary>
    [System.Serializable]
    public class InputMismatchException : RecognitionException
    {
        private const long serialVersionUID = 1532568338707443067L;

        public InputMismatchException([NotNull] Parser recognizer)
            : base(recognizer, ((ITokenStream)recognizer.InputStream), recognizer._ctx)
        {
            this.OffendingToken = recognizer.CurrentToken;
        }
    }
}
