/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import org.antlr.v4.runtime.ANTLRErrorListener;
import org.antlr.v4.runtime.Parser;
import org.antlr.v4.runtime.RecognitionException;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.misc.NotNull;

/**
 * This class represents profiling event information for a syntax error
 * identified during prediction. Syntax errors occur when the prediction
 * algorithm is unable to identify an alternative which would lead to a
 * successful parse.
 *
 * @see Parser#notifyErrorListeners(Token, String, RecognitionException)
 * @see ANTLRErrorListener#syntaxError
 *
 * @since 4.3
 */
public class ErrorInfo extends DecisionEventInfo {
	/**
	 * Constructs a new instance of the {@link ErrorInfo} class with the
	 * specified detailed syntax error information.
	 *
	 * @param decision The decision number
	 * @param state The final simulator state reached during prediction
	 * prior to reaching the {@link ATNSimulator#ERROR} state
	 * @param input The input token stream
	 * @param startIndex The start index for the current prediction
	 * @param stopIndex The index at which the syntax error was identified
	 */
	public ErrorInfo(int decision, @NotNull SimulatorState state, @NotNull TokenStream input,
					 int startIndex, int stopIndex)
	{
		super(decision, state, input, startIndex, stopIndex, state.useContext);
	}
}
