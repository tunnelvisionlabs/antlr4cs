// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>
    /// Start of
    /// <c>(A|B|...)+</c>
    /// loop. Technically a decision state, but
    /// we don't use for code generation; somebody might need it, so I'm defining
    /// it for completeness. In reality, the
    /// <see cref="PlusLoopbackState"/>
    /// node is the
    /// real decision-making note for
    /// <c>A+</c>
    /// .
    /// </summary>
    public sealed class PlusBlockStartState : BlockStartState
    {
        public PlusLoopbackState loopBackState;

        public override Antlr4.Runtime.Atn.StateType StateType
        {
            get
            {
                return Antlr4.Runtime.Atn.StateType.PlusBlockStart;
            }
        }
    }
}
