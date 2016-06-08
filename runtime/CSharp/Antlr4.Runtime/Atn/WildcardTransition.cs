// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    public sealed class WildcardTransition : Transition
    {
        public WildcardTransition(ATNState target)
            : base(target)
        {
        }

        public override Antlr4.Runtime.Atn.TransitionType TransitionType
        {
            get
            {
                return Antlr4.Runtime.Atn.TransitionType.Wildcard;
            }
        }

        public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
        {
            return symbol >= minVocabSymbol && symbol <= maxVocabSymbol;
        }

        [return: NotNull]
        public override string ToString()
        {
            return ".";
        }
    }
}
