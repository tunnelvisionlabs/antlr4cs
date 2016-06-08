// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <author>Sam Harwell</author>
    /// <since>4.2</since>
    public enum LexerActionType
    {
        Channel,
        Custom,
        Mode,
        More,
        PopMode,
        PushMode,
        Skip,
        Type
    }
}
