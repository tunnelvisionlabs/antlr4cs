// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    public sealed class EpsilonTransition : Transition
    {
        private readonly int outermostPrecedenceReturn;

        public EpsilonTransition([NotNull] ATNState target)
            : this(target, -1)
        {
        }

        public EpsilonTransition([NotNull] ATNState target, int outermostPrecedenceReturn)
            : base(target)
        {
            this.outermostPrecedenceReturn = outermostPrecedenceReturn;
        }

        /// <returns>
        /// the rule index of a precedence rule for which this transition is
        /// returning from, where the precedence value is 0; otherwise, -1.
        /// </returns>
        /// <seealso cref="ATNConfig.PrecedenceFilterSuppressed()"/>
        /// <seealso cref="ParserATNSimulator#applyPrecedenceFilter(ATNConfigSet,ParserRuleContext,PredictionContextCache)"></seealso>
        /// <since>4.4.1</since>
        public int OutermostPrecedenceReturn
        {
            get
            {
                return outermostPrecedenceReturn;
            }
        }

        public override Antlr4.Runtime.Atn.TransitionType TransitionType
        {
            get
            {
                return Antlr4.Runtime.Atn.TransitionType.Epsilon;
            }
        }

        public override bool IsEpsilon
        {
            get
            {
                return true;
            }
        }

        public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
        {
            return false;
        }

        [NotNull]
        public override string ToString()
        {
            return "epsilon";
        }
    }
}
