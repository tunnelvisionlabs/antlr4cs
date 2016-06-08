// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <author>Sam Harwell</author>
    public sealed class PrecedencePredicateTransition : AbstractPredicateTransition
    {
        public readonly int precedence;

        public PrecedencePredicateTransition(ATNState target, int precedence)
            : base(target)
        {
            this.precedence = precedence;
        }

        public override Antlr4.Runtime.Atn.TransitionType TransitionType
        {
            get
            {
                return Antlr4.Runtime.Atn.TransitionType.Precedence;
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

        public SemanticContext.PrecedencePredicate Predicate
        {
            get
            {
                return new SemanticContext.PrecedencePredicate(precedence);
            }
        }

        public override string ToString()
        {
            return precedence + " >= _p";
        }
    }
}
