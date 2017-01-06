/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.automata;

import org.antlr.v4.runtime.atn.ATN;
import org.antlr.v4.runtime.atn.ATNState;
import org.antlr.v4.runtime.atn.AtomTransition;
import org.antlr.v4.runtime.atn.BlockEndState;
import org.antlr.v4.runtime.atn.DecisionState;
import org.antlr.v4.runtime.atn.EpsilonTransition;
import org.antlr.v4.runtime.atn.NotSetTransition;
import org.antlr.v4.runtime.atn.RangeTransition;
import org.antlr.v4.runtime.atn.SetTransition;
import org.antlr.v4.runtime.atn.Transition;
import org.antlr.v4.runtime.misc.Interval;
import org.antlr.v4.runtime.misc.IntervalSet;
import org.antlr.v4.runtime.misc.NotNull;
import org.antlr.v4.tool.ErrorType;
import org.antlr.v4.tool.Grammar;
import org.antlr.v4.tool.Rule;

import java.util.List;

/**
 *
 * @author Sam Harwell
 */
public class ATNOptimizer {

	public static void optimize(@NotNull Grammar g, @NotNull ATN atn) {
		optimizeSets(g, atn);
		optimizeStates(atn);
	}

	private static void optimizeSets(Grammar g, ATN atn) {
		if (g.isParser()) {
			// parser codegen doesn't currently support SetTransition
			return;
		}

		int removedStates = 0;
		List<DecisionState> decisions = atn.decisionToState;
		for (DecisionState decision : decisions) {
			if (decision.ruleIndex >= 0) {
				Rule rule = g.getRule(decision.ruleIndex);
				if (Character.isLowerCase(rule.name.charAt(0))) {
					// parser codegen doesn't currently support SetTransition
					continue;
				}
			}

			IntervalSet setTransitions = new IntervalSet();
			for (int i = 0; i < decision.getNumberOfTransitions(); i++) {
				Transition epsTransition = decision.transition(i);
				if (!(epsTransition instanceof EpsilonTransition)) {
					continue;
				}

				if (epsTransition.target.getNumberOfTransitions() != 1) {
					continue;
				}

				Transition transition = epsTransition.target.transition(0);
				if (!(transition.target instanceof BlockEndState)) {
					continue;
				}

				if (transition instanceof NotSetTransition) {
					// TODO: not yet implemented
					continue;
				}

				if (transition instanceof AtomTransition
					|| transition instanceof RangeTransition
					|| transition instanceof SetTransition)
				{
					setTransitions.add(i);
				}
			}

			// due to min alt resolution policies, can only collapse sequential alts
			for (int i = setTransitions.getIntervals().size() - 1; i >= 0; i--) {
				Interval interval = setTransitions.getIntervals().get(i);
				if (interval.length() <= 1) {
					continue;
				}

				ATNState blockEndState = decision.transition(interval.a).target.transition(0).target;
				IntervalSet matchSet = new IntervalSet();
				for (int j = interval.a; j <= interval.b; j++) {
					Transition matchTransition = decision.transition(j).target.transition(0);
					if (matchTransition instanceof NotSetTransition) {
						throw new UnsupportedOperationException("Not yet implemented.");
					}
					IntervalSet set = matchTransition.label();
					int minElem = set.getMinElement();
					int maxElem = set.getMaxElement();
					for (int k = minElem; k <= maxElem; k++) {
						if (matchSet.contains(k)) {
							char setMin = (char) set.getMinElement();
							char setMax = (char) set.getMaxElement();
							// TODO: Token is missing (i.e. position in source will not be displayed).
							g.tool.errMgr.grammarError(ErrorType.CHARACTERS_COLLISION_IN_SET, g.fileName,
							                           null, (char) minElem + "-" + (char) maxElem, "[" + setMin + "-" + setMax + "]");
							break;
						}
					}
					matchSet.addAll(set);
				}

				Transition newTransition;
				if (matchSet.getIntervals().size() == 1) {
					if (matchSet.size() == 1) {
						newTransition = new AtomTransition(blockEndState, matchSet.getMinElement());
					}
					else {
						Interval matchInterval = matchSet.getIntervals().get(0);
						newTransition = new RangeTransition(blockEndState, matchInterval.a, matchInterval.b);
					}
				}
				else {
					newTransition = new SetTransition(blockEndState, matchSet);
				}

				decision.transition(interval.a).target.setTransition(0, newTransition);
				for (int j = interval.a + 1; j <= interval.b; j++) {
					Transition removed = decision.removeTransition(interval.a + 1);
					atn.removeState(removed.target);
					removedStates++;
				}
			}
		}

//		System.out.println("ATN optimizer removed " + removedStates + " states by collapsing sets.");
	}

	private static void optimizeStates(ATN atn) {
		List<ATNState> states = atn.states;

		int current = 0;
		for (int i = 0; i < states.size(); i++) {
			ATNState state = states.get(i);
			if (state == null) {
				continue;
			}

			if (i != current) {
				state.stateNumber = current;
				states.set(current, state);
				states.set(i, null);
			}

			current++;
		}

//		System.out.println("ATN optimizer removed " + (states.size() - current) + " null states.");
		states.subList(current, states.size()).clear();
	}

	private ATNOptimizer() {
	}

}
