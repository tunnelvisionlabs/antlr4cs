/* Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.dfa;

import org.antlr.v4.runtime.atn.LexerActionExecutor;
import org.antlr.v4.runtime.atn.ParserATNSimulator;

/**
 * Stores information about a {@link DFAState} which is an accept state under
 * some condition. Certain settings, such as
 * {@link ParserATNSimulator#getPredictionMode()}, may be used in addition to
 * this information to determine whether or not a particular state is an accept
 * state.
 *
 * @author Sam Harwell
 */
public class AcceptStateInfo {
	private final int prediction;
	private final LexerActionExecutor lexerActionExecutor;

	public AcceptStateInfo(int prediction) {
		this.prediction = prediction;
		this.lexerActionExecutor = null;
	}

	public AcceptStateInfo(int prediction, LexerActionExecutor lexerActionExecutor) {
		this.prediction = prediction;
		this.lexerActionExecutor = lexerActionExecutor;
	}

	/**
	 * Gets the prediction made by this accept state. Note that this value
	 * assumes the predicates, if any, in the {@link DFAState} evaluate to
	 * {@code true}. If predicate evaluation is enabled, the final prediction of
	 * the accept state will be determined by the result of predicate
	 * evaluation.
	 */
	public int getPrediction() {
		return prediction;
	}

	/**
	 * Gets the {@link LexerActionExecutor} which can be used to execute actions
	 * and/or commands after the lexer matches a token.
	 */
	public LexerActionExecutor getLexerActionExecutor() {
		return lexerActionExecutor;
	}
}
