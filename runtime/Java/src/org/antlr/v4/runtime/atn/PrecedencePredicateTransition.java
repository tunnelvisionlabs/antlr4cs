/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import org.antlr.v4.runtime.misc.NotNull;

/**
 *
 * @author Sam Harwell
 */
public final class PrecedencePredicateTransition extends AbstractPredicateTransition {
	public final int precedence;

	public PrecedencePredicateTransition(@NotNull ATNState target, int precedence) {
		super(target);
		this.precedence = precedence;
	}

	@Override
	public int getSerializationType() {
		return PRECEDENCE;
	}

	@Override
	public boolean isEpsilon() {
		return true;
	}

	@Override
	public boolean matches(int symbol, int minVocabSymbol, int maxVocabSymbol) {
		return false;
	}

	public SemanticContext.PrecedencePredicate getPredicate() {
		return new SemanticContext.PrecedencePredicate(precedence);
	}

	@Override
	public String toString() {
		return precedence + " >= _p";
	}

}
