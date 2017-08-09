// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    /// <summary>The default mechanism for creating tokens.</summary>
    /// <remarks>
    /// The default mechanism for creating tokens. It's used by default in Lexer and
    /// the error handling strategy (to create missing tokens).  Notifying the parser
    /// of a new factory means that it notifies it's token source and error strategy.
    /// </remarks>
    public interface ITokenFactory
    {
        /// <summary>
        /// This is the method used to create tokens in the lexer and in the
        /// error handling strategy.
        /// </summary>
        /// <remarks>
        /// This is the method used to create tokens in the lexer and in the
        /// error handling strategy. If text!=null, than the start and stop positions
        /// are wiped to -1 in the text override is set in the CommonToken.
        /// </remarks>
        [NotNull]
        IToken Create<_T0>(Tuple<_T0> source, int type, string text, int channel, int start, int stop, int line, int charPositionInLine)
            where _T0 : ITokenSource;

        /// <summary>Generically useful</summary>
        [NotNull]
        IToken Create(int type, string text);
    }
}
