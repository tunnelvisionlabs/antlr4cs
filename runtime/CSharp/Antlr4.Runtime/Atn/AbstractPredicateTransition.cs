// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <author>Sam Harwell</author>
    public abstract class AbstractPredicateTransition : Transition
    {
        public AbstractPredicateTransition(ATNState target)
            : base(target)
        {
        }
    }
}
