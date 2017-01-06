/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.test.tool;

import org.antlr.v4.Tool;
import org.antlr.v4.runtime.Parser;
import org.antlr.v4.runtime.ParserRuleContext;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.atn.ATN;
import org.antlr.v4.runtime.atn.ATNState;
import org.antlr.v4.runtime.atn.DecisionState;
import org.antlr.v4.runtime.atn.ParserATNSimulator;
import org.antlr.v4.runtime.misc.NotNull;
import org.antlr.v4.runtime.misc.Nullable;
import org.antlr.v4.tool.Grammar;

public class ParserInterpreterForTesting {
	public static class DummyParser extends Parser {
		public final ATN atn;

		public Grammar g;
		public DummyParser(Grammar g, ATN atn, TokenStream input) {
			super(input);
			this.g = g;
			this.atn = atn;
		}

		@Override
		public String getGrammarFileName() {
			throw new UnsupportedOperationException("not implemented");
		}

		@Override
		public String[] getRuleNames() {
			return g.rules.keySet().toArray(new String[g.rules.size()]);
		}

		@Override
		@Deprecated
		public String[] getTokenNames() {
			return g.getTokenNames();
		}

		@Override
		public ATN getATN() {
			return atn;
		}
	}

	protected Grammar g;
	protected ParserATNSimulator atnSimulator;
	protected TokenStream input;

	public ParserInterpreterForTesting(@NotNull Grammar g) {
		this.g = g;
	}

	public ParserInterpreterForTesting(@NotNull Grammar g, @NotNull TokenStream input) {
		Tool antlr = new Tool();
		antlr.process(g,false);
		atnSimulator = new ParserATNSimulator(new DummyParser(g, g.atn, input), g.atn);
	}

	public int adaptivePredict(@NotNull TokenStream input, int decision,
							   @Nullable ParserRuleContext outerContext)
	{
		return atnSimulator.adaptivePredict(input, decision, outerContext);
	}

	public int matchATN(@NotNull TokenStream input,
						@NotNull ATNState startState)
	{
		if (startState.getNumberOfTransitions() == 1) {
			return 1;
		}
		else if (startState instanceof DecisionState) {
			return atnSimulator.adaptivePredict(input, ((DecisionState)startState).decision, null, false);
		}
		else if (startState.getNumberOfTransitions() > 0) {
			return 1;
		}
		else {
			return -1;
		}
	}

	public ParserATNSimulator getATNSimulator() {
		return atnSimulator;
	}

}
