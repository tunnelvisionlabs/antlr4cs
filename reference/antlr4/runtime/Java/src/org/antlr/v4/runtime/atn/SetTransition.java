/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.misc.IntervalSet;
import org.antlr.v4.runtime.misc.NotNull;
import org.antlr.v4.runtime.misc.Nullable;

/** A transition containing a set of values. */
public class SetTransition extends Transition {
	@NotNull
	public final IntervalSet set;

	// TODO (sam): should we really allow null here?
	public SetTransition(@NotNull ATNState target, @Nullable IntervalSet set) {
		super(target);
		if ( set == null ) set = IntervalSet.of(Token.INVALID_TYPE);
		this.set = set;
	}

	@Override
	public TransitionType getSerializationType() {
		return TransitionType.SET;
	}

	@Override
	@NotNull
	public IntervalSet label() { return set; }

	@Override
	public boolean matches(int symbol, int minVocabSymbol, int maxVocabSymbol) {
		return set.contains(symbol);
	}

	@Override
	@NotNull
	public String toString() {
		return set.toString();
	}
}
