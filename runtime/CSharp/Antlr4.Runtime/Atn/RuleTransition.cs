// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    public sealed class RuleTransition : Transition
    {
        /// <summary>Ptr to the rule definition object for this rule ref</summary>
        public readonly int ruleIndex;

        public readonly int precedence;

        /// <summary>What node to begin computations following ref to rule</summary>
        [NotNull]
        public ATNState followState;

        public bool tailCall;

        public bool optimizedTailCall;

        [Obsolete(@"UseRuleTransition(RuleStartState, int, int, ATNState) instead.")]
        public RuleTransition(RuleStartState ruleStart, int ruleIndex, ATNState followState)
            : this(ruleStart, ruleIndex, 0, followState)
        {
        }

        public RuleTransition(RuleStartState ruleStart, int ruleIndex, int precedence, ATNState followState)
            : base(ruleStart)
        {
            // no Rule object at runtime
            this.ruleIndex = ruleIndex;
            this.precedence = precedence;
            this.followState = followState;
        }

        public override Antlr4.Runtime.Atn.TransitionType TransitionType
        {
            get
            {
                return Antlr4.Runtime.Atn.TransitionType.Rule;
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
    }
}
