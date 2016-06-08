// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>The block that begins a closure loop.</summary>
    /// <remarks>The block that begins a closure loop.</remarks>
    public sealed class StarBlockStartState : BlockStartState
    {
        public override Antlr4.Runtime.Atn.StateType StateType
        {
            get
            {
                return Antlr4.Runtime.Atn.StateType.StarBlockStart;
            }
        }
    }
}
