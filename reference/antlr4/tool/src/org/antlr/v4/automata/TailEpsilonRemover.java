/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.automata;

import org.antlr.v4.runtime.atn.ATN;
import org.antlr.v4.runtime.atn.ATNState;
import org.antlr.v4.runtime.atn.BlockEndState;
import org.antlr.v4.runtime.atn.EpsilonTransition;
import org.antlr.v4.runtime.atn.PlusLoopbackState;
import org.antlr.v4.runtime.atn.RuleTransition;
import org.antlr.v4.runtime.atn.StarLoopbackState;
import org.antlr.v4.runtime.atn.StateType;
import org.antlr.v4.runtime.atn.Transition;
import org.antlr.v4.runtime.misc.NotNull;

/**
 *
 * @author Terence Parr
 */
public class TailEpsilonRemover extends ATNVisitor {
	@NotNull
	private final ATN _atn;

	public TailEpsilonRemover(@NotNull ATN atn) {
		this._atn = atn;
	}

	@Override
	public void visitState(@NotNull ATNState p) {
		if (p.getStateType() == StateType.BASIC && p.getNumberOfTransitions() == 1) {
			ATNState q = p.transition(0).target;
			if (p.transition(0) instanceof RuleTransition) {
				q = ((RuleTransition) p.transition(0)).followState;
			}
			if (q.getStateType() == StateType.BASIC) {
				// we have p-x->q for x in {rule, action, pred, token, ...}
				// if edge out of q is single epsilon to block end
				// we can strip epsilon p-x->q-eps->r
				Transition trans = q.transition(0);
				if (q.getNumberOfTransitions() == 1 && trans instanceof EpsilonTransition) {
					ATNState r = trans.target;
					if (r instanceof BlockEndState || r instanceof PlusLoopbackState || r instanceof StarLoopbackState) {
						// skip over q
						if (p.transition(0) instanceof RuleTransition) {
							((RuleTransition) p.transition(0)).followState = r;
						} else {
							p.transition(0).target = r;
						}
						_atn.removeState(q);
					}
				}
			}
		}
	}
}
