/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.runtime;

import org.antlr.v4.runtime.misc.Nullable;

/**
 * This class extends {@link ParserRuleContext} by allowing the value of
 * {@link #getRuleIndex} to be explicitly set for the context.
 *
 * <p>
 * {@link ParserRuleContext} does not include field storage for the rule index
 * since the context classes created by the code generator override the
 * {@link #getRuleIndex} method to return the correct value for that context.
 * Since the parser interpreter does not use the context classes generated for a
 * parser, this class (with slightly more memory overhead per node) is used to
 * provide equivalent functionality.</p>
 */
public class InterpreterRuleContext extends ParserRuleContext {
	/**
	 * This is the backing field for {@link #getRuleIndex}.
	 */
	private final int ruleIndex;

	/**
	 * Constructs a new {@link InterpreterRuleContext} with the specified
	 * parent, invoking state, and rule index.
	 *
	 * @param parent The parent context.
	 * @param invokingStateNumber The invoking state number.
	 * @param ruleIndex The rule index for the current context.
	 */
	public InterpreterRuleContext(@Nullable ParserRuleContext parent,
								  int invokingStateNumber,
								  int ruleIndex)
	{
		super(parent, invokingStateNumber);
		this.ruleIndex = ruleIndex;
	}

	private InterpreterRuleContext(int ruleIndex) {
		this.ruleIndex = ruleIndex;
	}

	@Override
	public int getRuleIndex() {
		return ruleIndex;
	}
}
