/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import org.antlr.v4.runtime.misc.NotNull;

public class SingletonPredictionContext extends PredictionContext {

	@NotNull
	public final PredictionContext parent;
	public final int returnState;

	/*package*/ SingletonPredictionContext(@NotNull PredictionContext parent, int returnState) {
		super(calculateHashCode(parent, returnState));
		assert returnState != EMPTY_FULL_STATE_KEY && returnState != EMPTY_LOCAL_STATE_KEY;
		this.parent = parent;
		this.returnState = returnState;
	}

	@Override
	public PredictionContext getParent(int index) {
		assert index == 0;
		return parent;
	}

	@Override
	public int getReturnState(int index) {
		assert index == 0;
		return returnState;
	}

	@Override
	public int findReturnState(int returnState) {
		return this.returnState == returnState ? 0 : -1;
	}

	@Override
	public int size() {
		return 1;
	}

	@Override
	public boolean isEmpty() {
		return false;
	}

	@Override
	public boolean hasEmpty() {
		return false;
	}

	@Override
	public PredictionContext appendContext(PredictionContext suffix, PredictionContextCache contextCache) {
		return contextCache.getChild(parent.appendContext(suffix, contextCache), returnState);
	}

	@Override
	protected PredictionContext addEmptyContext() {
		PredictionContext[] parents = new PredictionContext[] { parent, EMPTY_FULL };
		int[] returnStates = new int[] { returnState, EMPTY_FULL_STATE_KEY };
		return new ArrayPredictionContext(parents, returnStates);
	}

	@Override
	protected PredictionContext removeEmptyContext() {
		return this;
	}

	@Override
	public boolean equals(Object o) {
		if (o == this) {
			return true;
		}
		else if (!(o instanceof SingletonPredictionContext)) {
			return false;
		}

		SingletonPredictionContext other = (SingletonPredictionContext)o;
		if (this.hashCode() != other.hashCode()) {
			return false;
		}

		return returnState == other.returnState
			&& parent.equals(other.parent);
	}

}
