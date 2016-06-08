// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>
    /// Decision state for
    /// <c>A+</c>
    /// and
    /// <c>(A|B)+</c>
    /// .  It has two transitions:
    /// one to the loop back to start of the block and one to exit.
    /// </summary>
    public sealed class PlusLoopbackState : DecisionState
    {
        public override Antlr4.Runtime.Atn.StateType StateType
        {
            get
            {
                return Antlr4.Runtime.Atn.StateType.PlusLoopBack;
            }
        }
    }
}
