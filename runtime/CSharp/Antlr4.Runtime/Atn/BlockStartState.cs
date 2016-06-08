// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>
    /// The start of a regular
    /// <c>(...)</c>
    /// block.
    /// </summary>
    public abstract class BlockStartState : DecisionState
    {
        public BlockEndState endState;
    }
}
