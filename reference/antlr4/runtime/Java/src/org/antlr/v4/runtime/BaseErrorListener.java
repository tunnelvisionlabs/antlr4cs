/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.runtime;

import org.antlr.v4.runtime.atn.ATNConfigSet;
import org.antlr.v4.runtime.atn.SimulatorState;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.NotNull;
import org.antlr.v4.runtime.misc.Nullable;

import java.util.BitSet;

/**
 * Provides an empty default implementation of {@link ANTLRErrorListener}. The
 * default implementation of each method does nothing, but can be overridden as
 * necessary.
 *
 * @author Sam Harwell
 */
public class BaseErrorListener implements ParserErrorListener {
	@Override
	public <T extends Token> void syntaxError(@NotNull Recognizer<T, ?> recognizer,
											  @Nullable T offendingSymbol,
											  int line,
											  int charPositionInLine,
											  @NotNull String msg,
											  @Nullable RecognitionException e)
	{
	}

	@Override
	public void reportAmbiguity(@NotNull Parser recognizer,
								@NotNull DFA dfa,
								int startIndex,
								int stopIndex,
								boolean exact,
								@Nullable BitSet ambigAlts,
								@NotNull ATNConfigSet configs)
	{
	}

	@Override
	public void reportAttemptingFullContext(@NotNull Parser recognizer,
											@NotNull DFA dfa,
											int startIndex,
											int stopIndex,
											@Nullable BitSet conflictingAlts,
											@NotNull SimulatorState conflictState)
	{
	}

	@Override
	public void reportContextSensitivity(@NotNull Parser recognizer,
										 @NotNull DFA dfa,
										 int startIndex,
										 int stopIndex,
										 int prediction,
										 @NotNull SimulatorState acceptState)
	{
	}
}
