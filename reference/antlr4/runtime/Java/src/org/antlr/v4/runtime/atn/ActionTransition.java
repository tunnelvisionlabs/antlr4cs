/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import org.antlr.v4.runtime.misc.NotNull;

public final class ActionTransition extends Transition {
	public final int ruleIndex;
	public final int actionIndex;
	public final boolean isCtxDependent; // e.g., $i ref in action

	public ActionTransition(@NotNull ATNState target, int ruleIndex) {
		this(target, ruleIndex, -1, false);
	}

	public ActionTransition(@NotNull ATNState target, int ruleIndex, int actionIndex, boolean isCtxDependent) {
		super(target);
		this.ruleIndex = ruleIndex;
		this.actionIndex = actionIndex;
		this.isCtxDependent = isCtxDependent;
	}

	@Override
	public TransitionType getSerializationType() {
		return TransitionType.ACTION;
	}

	@Override
	public boolean isEpsilon() {
		return true; // we are to be ignored by analysis 'cept for predicates
	}

	@Override
	public boolean matches(int symbol, int minVocabSymbol, int maxVocabSymbol) {
		return false;
	}

	@Override
	public String toString() {
		return "action_"+ruleIndex+":"+actionIndex;
	}
}
