/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

/**
 *
 * @author Sam Harwell
 */
public class OrderedATNConfigSet extends ATNConfigSet {

	public OrderedATNConfigSet() {
	}

	public OrderedATNConfigSet(ATNConfigSet set, boolean readonly) {
		super(set, readonly);
	}

	@Override
	public ATNConfigSet clone(boolean readonly) {
		OrderedATNConfigSet copy = new OrderedATNConfigSet(this, readonly);
		if (!readonly && this.isReadOnly()) {
			copy.addAll(this);
		}

		return copy;
	}

	@Override
	protected long getKey(ATNConfig e) {
		return e.hashCode();
	}

	@Override
	protected boolean canMerge(ATNConfig left, long leftKey, ATNConfig right) {
		return left.equals(right);
	}

}
