// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>
    /// Terminal node of a simple
    /// <c>(a|b|c)</c>
    /// block.
    /// </summary>
    public sealed class BlockEndState : ATNState
    {
        public BlockStartState startState;

        public override Antlr4.Runtime.Atn.StateType StateType
        {
            get
            {
                return Antlr4.Runtime.Atn.StateType.BlockEnd;
            }
        }
    }
}
