/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import org.antlr.v4.runtime.misc.IntervalSet;
import org.antlr.v4.runtime.misc.NotNull;
import org.antlr.v4.runtime.misc.Nullable;

import java.util.Arrays;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/** An ATN transition between any two ATN states.  Subclasses define
 *  atom, set, epsilon, action, predicate, rule transitions.
 *
 *  <p>This is a one way link.  It emanates from a state (usually via a list of
 *  transitions) and has a target state.</p>
 *
 *  <p>Since we never have to change the ATN transitions once we construct it,
 *  we can fix these transitions as specific classes. The DFA transitions
 *  on the other hand need to update the labels as it adds transitions to
 *  the states. We'll use the term Edge for the DFA to distinguish them from
 *  ATN transitions.</p>
 */
public abstract class Transition {

	public static final List<String> serializationNames =
		Collections.unmodifiableList(Arrays.asList(
			"INVALID",
			"EPSILON",
			"RANGE",
			"RULE",
			"PREDICATE",
			"ATOM",
			"ACTION",
			"SET",
			"NOT_SET",
			"WILDCARD",
			"PRECEDENCE"
		));

	/**
	 * @sharpen.ignore
	 */
	@SuppressWarnings("serial")
	public static final Map<Class<? extends Transition>, TransitionType> serializationTypes =
		Collections.unmodifiableMap(new HashMap<Class<? extends Transition>, TransitionType>() {{
			put(EpsilonTransition.class, TransitionType.EPSILON);
			put(RangeTransition.class, TransitionType.RANGE);
			put(RuleTransition.class, TransitionType.RULE);
			put(PredicateTransition.class, TransitionType.PREDICATE);
			put(AtomTransition.class, TransitionType.ATOM);
			put(ActionTransition.class, TransitionType.ACTION);
			put(SetTransition.class, TransitionType.SET);
			put(NotSetTransition.class, TransitionType.NOT_SET);
			put(WildcardTransition.class, TransitionType.WILDCARD);
			put(PrecedencePredicateTransition.class, TransitionType.PRECEDENCE);
		}});

	/** The target of this transition. */
	@NotNull
	public ATNState target;

	protected Transition(@NotNull ATNState target) {
		if (target == null) {
			throw new NullPointerException("target cannot be null.");
		}

		this.target = target;
	}

	/**
	 * @sharpen.property TransitionType
	 */
	public abstract TransitionType getSerializationType();

	/**
	 * Determines if the transition is an "epsilon" transition.
	 *
	 * <p>The default implementation returns {@code false}.</p>
	 *
	 * @return {@code true} if traversing this transition in the ATN does not
	 * consume an input symbol; otherwise, {@code false} if traversing this
	 * transition consumes (matches) an input symbol.
	 * 
	 * @sharpen.property
	 */
	public boolean isEpsilon() {
		return false;
	}

	/**
	 * @sharpen.property
	 */
	@Nullable
	public IntervalSet label() { return null; }

	public abstract boolean matches(int symbol, int minVocabSymbol, int maxVocabSymbol);
}
