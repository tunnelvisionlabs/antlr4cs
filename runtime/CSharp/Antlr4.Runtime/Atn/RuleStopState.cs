// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>The last node in the ATN for a rule, unless that rule is the start symbol.</summary>
    /// <remarks>
    /// The last node in the ATN for a rule, unless that rule is the start symbol.
    /// In that case, there is one transition to EOF. Later, we might encode
    /// references to all calls to this rule to compute FOLLOW sets for
    /// error handling.
    /// </remarks>
    public sealed class RuleStopState : ATNState
    {
        public override int NonStopStateNumber
        {
            get
            {
                return -1;
            }
        }

        public override Antlr4.Runtime.Atn.StateType StateType
        {
            get
            {
                return Antlr4.Runtime.Atn.StateType.RuleStop;
            }
        }
    }
}
