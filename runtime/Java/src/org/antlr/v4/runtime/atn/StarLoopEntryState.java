/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import org.antlr.v4.runtime.ParserInterpreter;
import org.antlr.v4.runtime.dfa.DFA;

import java.util.BitSet;

public final class StarLoopEntryState extends DecisionState {
	public StarLoopbackState loopBackState;

	/**
	 * Indicates whether this state can benefit from a precedence DFA during SLL
	 * decision making.
	 *
	 * <p>This is a computed property that is calculated during ATN deserialization
	 * and stored for use in {@link ParserATNSimulator} and
	 * {@link ParserInterpreter}.</p>
	 *
	 * @see DFA#isPrecedenceDfa()
	 */
	public boolean precedenceRuleDecision;

	/**
	 * For precedence decisions, this set marks states <em>S</em> which have all
	 * of the following characteristics:
	 *
	 * <ul>
	 * <li>One or more invocation sites of the current rule returns to
	 * <em>S</em>.</li>
	 * <li>The closure from <em>S</em> includes the current decision without
	 * passing through any rule invocations or stepping out of the current
	 * rule.</li>
	 * </ul>
	 *
	 * <p>This field is {@code null} when {@link #isPrecedenceDecision} is
	 * {@code false}.</p>
	 */
	public BitSet precedenceLoopbackStates;

	@Override
	public int getStateType() {
		return STAR_LOOP_ENTRY;
	}
}
