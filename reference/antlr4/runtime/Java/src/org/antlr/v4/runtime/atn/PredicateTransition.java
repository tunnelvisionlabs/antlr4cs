/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import org.antlr.v4.runtime.misc.NotNull;

/** TODO: this is old comment:
 *  A tree of semantic predicates from the grammar AST if label==SEMPRED.
 *  In the ATN, labels will always be exactly one predicate, but the DFA
 *  may have to combine a bunch of them as it collects predicates from
 *  multiple ATN configurations into a single DFA state.
 */
public final class PredicateTransition extends AbstractPredicateTransition {
	public final int ruleIndex;
	public final int predIndex;
	public final boolean isCtxDependent;  // e.g., $i ref in pred

	public PredicateTransition(@NotNull ATNState target, int ruleIndex, int predIndex, boolean isCtxDependent) {
		super(target);
		this.ruleIndex = ruleIndex;
		this.predIndex = predIndex;
		this.isCtxDependent = isCtxDependent;
	}

	@Override
	public TransitionType getSerializationType() {
		return TransitionType.PREDICATE;
	}

	@Override
	public boolean isEpsilon() { return true; }

	@Override
	public boolean matches(int symbol, int minVocabSymbol, int maxVocabSymbol) {
		return false;
	}

	/**
	 * @sharpen.property Predicate
	 */
    public SemanticContext.Predicate getPredicate() {
   		return new SemanticContext.Predicate(ruleIndex, predIndex, isCtxDependent);
   	}

	@Override
	@NotNull
	public String toString() {
		return "pred_"+ruleIndex+":"+predIndex;
	}

}
