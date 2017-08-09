// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    /// <summary>
    /// Indicates that the parser could not decide which of two or more paths
    /// to take based upon the remaining input.
    /// </summary>
    /// <remarks>
    /// Indicates that the parser could not decide which of two or more paths
    /// to take based upon the remaining input. It tracks the starting token
    /// of the offending input and also knows where the parser was
    /// in the various paths when the error. Reported by reportNoViableAlternative()
    /// </remarks>
    [System.Serializable]
    public class NoViableAltException : RecognitionException
    {
        private const long serialVersionUID = 5096000008992867052L;

        /// <summary>Which configurations did we try at input.index() that couldn't match input.LT(1)?</summary>
        [Nullable]
        private readonly ATNConfigSet deadEndConfigs;

        /// <summary>
        /// The token object at the start index; the input stream might
        /// not be buffering tokens so get a reference to it.
        /// </summary>
        /// <remarks>
        /// The token object at the start index; the input stream might
        /// not be buffering tokens so get a reference to it. (At the
        /// time the error occurred, of course the stream needs to keep a
        /// buffer all of the tokens but later we might not have access to those.)
        /// </remarks>
        [NotNull]
        private readonly IToken startToken;

        public NoViableAltException([NotNull] Parser recognizer)
            : this(recognizer, ((ITokenStream)recognizer.InputStream), recognizer.CurrentToken, recognizer.CurrentToken, null, recognizer._ctx)
        {
        }

        public NoViableAltException([NotNull] Recognizer<IToken, object> recognizer, [NotNull] ITokenStream input, [NotNull] IToken startToken, [NotNull] IToken offendingToken, [Nullable] ATNConfigSet deadEndConfigs, [NotNull] ParserRuleContext ctx)
            : base(recognizer, input, ctx)
        {
            // LL(1) error
            this.deadEndConfigs = deadEndConfigs;
            this.startToken = startToken;
            this.OffendingToken = offendingToken;
        }

        public virtual IToken StartToken
        {
            get
            {
                return startToken;
            }
        }

        public virtual ATNConfigSet DeadEndConfigs
        {
            get
            {
                return deadEndConfigs;
            }
        }
    }
}
