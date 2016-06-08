// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    public sealed class RangeTransition : Transition
    {
        public readonly int from;

        public readonly int to;

        public RangeTransition(ATNState target, int from, int to)
            : base(target)
        {
            this.from = from;
            this.to = to;
        }

        public override Antlr4.Runtime.Atn.TransitionType TransitionType
        {
            get
            {
                return Antlr4.Runtime.Atn.TransitionType.Range;
            }
        }

        public override IntervalSet Label
        {
            get
            {
                return IntervalSet.Of(from, to);
            }
        }

        public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
        {
            return symbol >= from && symbol <= to;
        }

        [return: NotNull]
        public override string ToString()
        {
            return "'" + (char)from + "'..'" + (char)to + "'";
        }
    }
}
