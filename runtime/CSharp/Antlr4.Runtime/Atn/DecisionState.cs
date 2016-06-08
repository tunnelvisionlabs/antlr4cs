// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    public abstract class DecisionState : ATNState
    {
        public int decision = -1;

        public bool nonGreedy;

        public bool sll;
    }
}
