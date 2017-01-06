/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import org.antlr.v4.runtime.atn.PredictionContextCache.IdentityCommutativePredictionContextOperands;
import org.antlr.v4.runtime.misc.NotNull;

import java.util.ArrayDeque;
import java.util.Arrays;
import java.util.Deque;
import java.util.HashSet;
import java.util.Set;

public class ArrayPredictionContext extends PredictionContext {

	@NotNull
	public final PredictionContext[] parents;
	@NotNull
	public final int[] returnStates;

	/*package*/ ArrayPredictionContext(@NotNull PredictionContext[] parents, int[] returnStates) {
		super(calculateHashCode(parents, returnStates));
		assert parents.length == returnStates.length;
		assert returnStates.length > 1 || returnStates[0] != EMPTY_FULL_STATE_KEY : "Should be using PredictionContext.EMPTY instead.";

		this.parents = parents;
		this.returnStates = returnStates;
	}

	/*package*/ ArrayPredictionContext(@NotNull PredictionContext[] parents, int[] returnStates, int hashCode) {
		super(hashCode);
		assert parents.length == returnStates.length;
		assert returnStates.length > 1 || returnStates[0] != EMPTY_FULL_STATE_KEY : "Should be using PredictionContext.EMPTY instead.";

		this.parents = parents;
		this.returnStates = returnStates;
	}

	@Override
	public PredictionContext getParent(int index) {
		return parents[index];
	}

	@Override
	public int getReturnState(int index) {
		return returnStates[index];
	}

	@Override
	public int findReturnState(int returnState) {
		return Arrays.binarySearch(returnStates, returnState);
	}

	@Override
	public int size() {
		return returnStates.length;
	}

	@Override
	public boolean isEmpty() {
		return false;
	}

	@Override
	public boolean hasEmpty() {
		return returnStates[returnStates.length - 1] == EMPTY_FULL_STATE_KEY;
	}

	@Override
	protected PredictionContext addEmptyContext() {
		if (hasEmpty()) {
			return this;
		}

		PredictionContext[] parents2 = Arrays.copyOf(parents, parents.length + 1);
		int[] returnStates2 = Arrays.copyOf(returnStates, returnStates.length + 1);
		parents2[parents2.length - 1] = PredictionContext.EMPTY_FULL;
		returnStates2[returnStates2.length - 1] = PredictionContext.EMPTY_FULL_STATE_KEY;
		return new ArrayPredictionContext(parents2, returnStates2);
	}

	@Override
	protected PredictionContext removeEmptyContext() {
		if (!hasEmpty()) {
			return this;
		}

		if (returnStates.length == 2) {
			return new SingletonPredictionContext(parents[0], returnStates[0]);
		}
		else {
			PredictionContext[] parents2 = Arrays.copyOf(parents, parents.length - 1);
			int[] returnStates2 = Arrays.copyOf(returnStates, returnStates.length - 1);
			return new ArrayPredictionContext(parents2, returnStates2);
		}
	}

	@Override
	public PredictionContext appendContext(PredictionContext suffix, PredictionContextCache contextCache) {
		return appendContext(this, suffix, new PredictionContext.IdentityHashMap());
	}

	private static PredictionContext appendContext(PredictionContext context, PredictionContext suffix, PredictionContext.IdentityHashMap visited) {
		if (suffix.isEmpty()) {
			if (isEmptyLocal(suffix)) {
				if (context.hasEmpty()) {
					return EMPTY_LOCAL;
				}
				
				throw new UnsupportedOperationException("what to do here?");
			}

			return context;
		}

		if (suffix.size() != 1) {
			throw new UnsupportedOperationException("Appending a tree suffix is not yet supported.");
		}

		PredictionContext result = visited.get(context);
		if (result == null) {
			if (context.isEmpty()) {
				result = suffix;
			}
			else {
				int parentCount = context.size();
				if (context.hasEmpty()) {
					parentCount--;
				}

				PredictionContext[] updatedParents = new PredictionContext[parentCount];
				int[] updatedReturnStates = new int[parentCount];
				for (int i = 0; i < parentCount; i++) {
					updatedReturnStates[i] = context.getReturnState(i);
				}

				for (int i = 0; i < parentCount; i++) {
					updatedParents[i] = appendContext(context.getParent(i), suffix, visited);
				}

				if (updatedParents.length == 1) {
					result = new SingletonPredictionContext(updatedParents[0], updatedReturnStates[0]);
				}
				else {
					assert updatedParents.length > 1;
					result = new ArrayPredictionContext(updatedParents, updatedReturnStates);
				}

				if (context.hasEmpty()) {
					result = PredictionContext.join(result, suffix);
				}
			}

			visited.put(context, result);
		}

		return result;
	}

	@Override
	public boolean equals(Object o) {
		if (this == o) {
			return true;
		}
		else if (!(o instanceof ArrayPredictionContext)) {
			return false;
		}

		if ( this.hashCode() != o.hashCode() ) {
			return false; // can't be same if hash is different
		}

		ArrayPredictionContext other = (ArrayPredictionContext)o;
		return equals(other, new HashSet<IdentityCommutativePredictionContextOperands>());
	}

	private boolean equals(ArrayPredictionContext other, Set<IdentityCommutativePredictionContextOperands> visited) {
		Deque<PredictionContext> selfWorkList = new ArrayDeque<PredictionContext>();
		Deque<PredictionContext> otherWorkList = new ArrayDeque<PredictionContext>();
		selfWorkList.push(this);
		otherWorkList.push(other);
		while (!selfWorkList.isEmpty()) {
			IdentityCommutativePredictionContextOperands operands = new IdentityCommutativePredictionContextOperands(selfWorkList.pop(), otherWorkList.pop());
			if (!visited.add(operands)) {
				continue;
			}

			int selfSize = operands.getX().size();
			if (selfSize == 0) {
				if (!operands.getX().equals(operands.getY())) {
					return false;
				}

				continue;
			}

			int otherSize = operands.getY().size();
			if (selfSize != otherSize) {
				return false;
			}

			for (int i = 0; i < selfSize; i++) {
				if (operands.getX().getReturnState(i) != operands.getY().getReturnState(i)) {
					return false;
				}

				PredictionContext selfParent = operands.getX().getParent(i);
				PredictionContext otherParent = operands.getY().getParent(i);
				if (selfParent.hashCode() != otherParent.hashCode()) {
					return false;
				}

				if (selfParent != otherParent) {
					selfWorkList.push(selfParent);
					otherWorkList.push(otherParent);
				}
			}
		}

		return true;
	}

}
