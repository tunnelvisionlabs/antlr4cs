// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

using System.Globalization;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime
{
    /// <summary>A semantic predicate failed during validation.</summary>
    /// <remarks>
    /// A semantic predicate failed during validation.  Validation of predicates
    /// occurs when normally parsing the alternative just like matching a token.
    /// Disambiguating predicate evaluation occurs when we test a predicate during
    /// prediction.
    /// </remarks>
    [System.Serializable]
    public class FailedPredicateException : RecognitionException
    {
        private const long serialVersionUID = 5379330841495778709L;

        private readonly int ruleIndex;

        private readonly int predicateIndex;

        private readonly string predicate;

        public FailedPredicateException(Parser recognizer)
            : this(recognizer, null)
        {
        }

        public FailedPredicateException(Parser recognizer, string predicate)
            : this(recognizer, predicate, null)
        {
        }

        public FailedPredicateException(Parser recognizer, string predicate, string message)
            : base(FormatMessage(predicate, message), recognizer, ((ITokenStream)recognizer.InputStream), recognizer._ctx)
        {
            ATNState s = recognizer.Interpreter.atn.states[recognizer.State];
            AbstractPredicateTransition trans = (AbstractPredicateTransition)s.Transition(0);
            if (trans is PredicateTransition)
            {
                this.ruleIndex = ((PredicateTransition)trans).ruleIndex;
                this.predicateIndex = ((PredicateTransition)trans).predIndex;
            }
            else
            {
                this.ruleIndex = 0;
                this.predicateIndex = 0;
            }
            this.predicate = predicate;
            this.OffendingToken = recognizer.CurrentToken;
        }

        public virtual int RuleIndex
        {
            get
            {
                return ruleIndex;
            }
        }

        public virtual int PredIndex
        {
            get
            {
                return predicateIndex;
            }
        }

        [Nullable]
        public virtual string Predicate
        {
            get
            {
                return predicate;
            }
        }

        [return: NotNull]
        private static string FormatMessage(string predicate, string message)
        {
            if (message != null)
            {
                return message;
            }
            return string.Format(CultureInfo.CurrentCulture, "failed predicate: {{{0}}}?", predicate);
        }
    }
}
