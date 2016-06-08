// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>Represents the type of recognizer an ATN applies to.</summary>
    /// <author>Sam Harwell</author>
    public enum ATNType
    {
        Lexer,
        Parser
    }
}
