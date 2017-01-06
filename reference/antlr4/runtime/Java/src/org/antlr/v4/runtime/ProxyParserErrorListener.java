/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.runtime;

import org.antlr.v4.runtime.atn.ATNConfigSet;
import org.antlr.v4.runtime.atn.SimulatorState;
import org.antlr.v4.runtime.dfa.DFA;

import java.util.BitSet;
import java.util.Collection;

/**
 *
 * @author Sam Harwell
 */
public class ProxyParserErrorListener extends ProxyErrorListener<Token> implements ParserErrorListener {
	public ProxyParserErrorListener(Collection<? extends ANTLRErrorListener<? super Token>> delegates) {
		super(delegates);
	}

	@Override
	public void reportAmbiguity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, boolean exact, BitSet ambigAlts, ATNConfigSet configs) {
		for (ANTLRErrorListener<? super Token> listener : getDelegates()) {
			if (!(listener instanceof ParserErrorListener)) {
				continue;
			}

			ParserErrorListener parserErrorListener = (ParserErrorListener)listener;
			parserErrorListener.reportAmbiguity(recognizer, dfa, startIndex, stopIndex, exact, ambigAlts, configs);
		}
	}

	@Override
	public void reportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState) {
		for (ANTLRErrorListener<? super Token> listener : getDelegates()) {
			if (!(listener instanceof ParserErrorListener)) {
				continue;
			}

			ParserErrorListener parserErrorListener = (ParserErrorListener)listener;
			parserErrorListener.reportAttemptingFullContext(recognizer, dfa, startIndex, stopIndex, conflictingAlts, conflictState);
		}
	}

	@Override
	public void reportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState) {
		for (ANTLRErrorListener<? super Token> listener : getDelegates()) {
			if (!(listener instanceof ParserErrorListener)) {
				continue;
			}

			ParserErrorListener parserErrorListener = (ParserErrorListener)listener;
			parserErrorListener.reportContextSensitivity(recognizer, dfa, startIndex, stopIndex, prediction, acceptState);
		}
	}
}
