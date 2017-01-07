/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import org.antlr.v4.runtime.misc.IntervalSet;
import org.antlr.v4.runtime.misc.NotNull;

public final class RangeTransition extends Transition {
	public final int from;
	public final int to;

	public RangeTransition(@NotNull ATNState target, int from, int to) {
		super(target);
		this.from = from;
		this.to = to;
	}

	@Override
	public TransitionType getSerializationType() {
		return TransitionType.RANGE;
	}

	@Override
	@NotNull
	public IntervalSet label() { return IntervalSet.of(from, to); }

	@Override
	public boolean matches(int symbol, int minVocabSymbol, int maxVocabSymbol) {
		return symbol >= from && symbol <= to;
	}

	@Override
	@NotNull
	public String toString() {
		return "'"+(char)from+"'..'"+(char)to+"'";
	}
}
