// Copyright (c) Terence Parr, Sam Harwell. All Rights Reserved.
// Licensed under the BSD License. See LICENSE.txt in the project root for license information.

/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>An ATN transition between any two ATN states.</summary>
    /// <remarks>
    /// An ATN transition between any two ATN states.  Subclasses define
    /// atom, set, epsilon, action, predicate, rule transitions.
    /// <p>This is a one way link.  It emanates from a state (usually via a list of
    /// transitions) and has a target state.</p>
    /// <p>Since we never have to change the ATN transitions once we construct it,
    /// we can fix these transitions as specific classes. The DFA transitions
    /// on the other hand need to update the labels as it adds transitions to
    /// the states. We'll use the term Edge for the DFA to distinguish them from
    /// ATN transitions.</p>
    /// </remarks>
    public abstract class Transition
    {
        public static readonly IList<string> serializationNames = Antlr4.Runtime.Sharpen.Collections.UnmodifiableList(Arrays.AsList("INVALID", "EPSILON", "RANGE", "RULE", "PREDICATE", "ATOM", "ACTION", "SET", "NOT_SET", "WILDCARD", "PRECEDENCE"));

        /// <summary>The target of this transition.</summary>
        [NotNull]
        public ATNState target;

        protected internal Transition([NotNull] ATNState target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target cannot be null.");
            }
            this.target = target;
        }

        public abstract Antlr4.Runtime.Atn.TransitionType TransitionType
        {
            get;
        }

        /// <summary>Determines if the transition is an "epsilon" transition.</summary>
        /// <remarks>
        /// Determines if the transition is an "epsilon" transition.
        /// <p>The default implementation returns
        /// <see langword="false"/>
        /// .</p>
        /// </remarks>
        /// <returns>
        /// 
        /// <see langword="true"/>
        /// if traversing this transition in the ATN does not
        /// consume an input symbol; otherwise,
        /// <see langword="false"/>
        /// if traversing this
        /// transition consumes (matches) an input symbol.
        /// </returns>
        public virtual bool IsEpsilon
        {
            get
            {
                return false;
            }
        }

        public virtual IntervalSet Label
        {
            get
            {
                return null;
            }
        }

        public abstract bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol);
    }
}
