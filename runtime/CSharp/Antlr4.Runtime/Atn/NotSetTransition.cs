// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    public sealed class NotSetTransition : SetTransition
    {
        public NotSetTransition(ATNState target, IntervalSet set)
            : base(target, set)
        {
        }

        public override Antlr4.Runtime.Atn.TransitionType TransitionType
        {
            get
            {
                return Antlr4.Runtime.Atn.TransitionType.NotSet;
            }
        }

        public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
        {
            return symbol >= minVocabSymbol && symbol <= maxVocabSymbol && !base.Matches(symbol, minVocabSymbol, maxVocabSymbol);
        }

        public override string ToString()
        {
            return '~' + base.ToString();
        }
    }
}
