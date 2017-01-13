/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

public final class StarLoopbackState extends ATNState {
	/**
	 * @sharpen.property LoopEntryState
	 */
	public final StarLoopEntryState getLoopEntryState() {
		return (StarLoopEntryState)transition(0).target;
	}

	@Override
	public StateType getStateType() {
		return StateType.STAR_LOOP_BACK;
	}
}
