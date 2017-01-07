/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import org.antlr.v4.runtime.misc.Utils;

import java.util.BitSet;

/**
 * This class stores information about a configuration conflict.
 *
 * @author Sam Harwell
 */
public class ConflictInfo {
	private final BitSet conflictedAlts;

	private final boolean exact;

	public ConflictInfo(BitSet conflictedAlts, boolean exact) {
		this.conflictedAlts = conflictedAlts;
		this.exact = exact;
	}

	/**
	 * Gets the set of conflicting alternatives for the configuration set.
	 *
	 * @sharpen.property ConflictedAlts
	 */
	public final BitSet getConflictedAlts() {
		return conflictedAlts;
	}

	/**
	 * Gets whether or not the configuration conflict is an exact conflict.
	 * An exact conflict occurs when the prediction algorithm determines that
	 * the represented alternatives for a particular configuration set cannot be
	 * further reduced by consuming additional input. After reaching an exact
	 * conflict during an SLL prediction, only switch to full-context prediction
	 * could reduce the set of viable alternatives. In LL prediction, an exact
	 * conflict indicates a true ambiguity in the input.
	 *
	 * <p>
	 * For the {@link PredictionMode#LL_EXACT_AMBIG_DETECTION} prediction mode,
	 * accept states are conflicting but not exact are treated as non-accept
	 * states.</p>
	 *
	 * @sharpen.property
	 */
	public final boolean isExact() {
		return exact;
	}

	@Override
	public boolean equals(Object obj) {
		if (obj == this) {
			return true;
		}
		else if (!(obj instanceof ConflictInfo)) {
			return false;
		}

		ConflictInfo other = (ConflictInfo)obj;
		return isExact() == other.isExact()
			&& Utils.equals(getConflictedAlts(), other.getConflictedAlts());
	}

	@Override
	public int hashCode() {
		return getConflictedAlts().hashCode();
	}
}
