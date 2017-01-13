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

	/**
	 * @sharpen.property Default
	 */
	@NotNull
	public static ATNDeserializationOptions getDefaultOptions() {
		return defaultOptions;
	}

	/**
	 * @sharpen.property IsReadOnly
	 */
	public final boolean isReadOnly() {
		return readOnly;
	}

	public final void makeReadOnly() {
		readOnly = true;
	}

	/**
	 * @sharpen.property VerifyAtn
	 */
	public final boolean isVerifyATN() {
		return verifyATN;
	}

	/**
	 * @sharpen.property VerifyAtn
	 */
	public final void setVerifyATN(boolean verifyATN) {
		throwIfReadOnly();
		this.verifyATN = verifyATN;
	}

	/**
	 * @sharpen.property GenerateRuleBypassTransitions
	 */
	public final boolean isGenerateRuleBypassTransitions() {
		return generateRuleBypassTransitions;
	}

	/**
	 * @sharpen.property GenerateRuleBypassTransitions
	 */
	public final void setGenerateRuleBypassTransitions(boolean generateRuleBypassTransitions) {
		throwIfReadOnly();
		this.generateRuleBypassTransitions = generateRuleBypassTransitions;
	}

	/**
	 * @sharpen.property Optimize
	 */
	public final boolean isOptimize() {
		return optimize;
	}

	/**
	 * @sharpen.property Optimize
	 */
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
