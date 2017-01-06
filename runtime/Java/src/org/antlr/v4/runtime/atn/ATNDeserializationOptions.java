/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.atn;

import org.antlr.v4.runtime.misc.NotNull;

/**
 *
 * @author Sam Harwell
 */
public class ATNDeserializationOptions {
	private static final ATNDeserializationOptions defaultOptions;
	static {
		defaultOptions = new ATNDeserializationOptions();
		defaultOptions.makeReadOnly();
	}

	private boolean readOnly;
	private boolean verifyATN;
	private boolean generateRuleBypassTransitions;
	private boolean optimize;

	public ATNDeserializationOptions() {
		this.verifyATN = true;
		this.generateRuleBypassTransitions = false;
		this.optimize = true;
	}

	public ATNDeserializationOptions(ATNDeserializationOptions options) {
		this.verifyATN = options.verifyATN;
		this.generateRuleBypassTransitions = options.generateRuleBypassTransitions;
		this.optimize = options.optimize;
	}

	@NotNull
	public static ATNDeserializationOptions getDefaultOptions() {
		return defaultOptions;
	}

	public final boolean isReadOnly() {
		return readOnly;
	}

	public final void makeReadOnly() {
		readOnly = true;
	}

	public final boolean isVerifyATN() {
		return verifyATN;
	}

	public final void setVerifyATN(boolean verifyATN) {
		throwIfReadOnly();
		this.verifyATN = verifyATN;
	}

	public final boolean isGenerateRuleBypassTransitions() {
		return generateRuleBypassTransitions;
	}

	public final void setGenerateRuleBypassTransitions(boolean generateRuleBypassTransitions) {
		throwIfReadOnly();
		this.generateRuleBypassTransitions = generateRuleBypassTransitions;
	}

	public final boolean isOptimize() {
		return optimize;
	}

	public final void setOptimize(boolean optimize) {
		throwIfReadOnly();
		this.optimize = optimize;
	}

	protected void throwIfReadOnly() {
		if (isReadOnly()) {
			throw new IllegalStateException("The object is read only.");
		}
	}
}
