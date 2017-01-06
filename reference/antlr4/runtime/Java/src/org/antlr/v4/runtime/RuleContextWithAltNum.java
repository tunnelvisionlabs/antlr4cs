/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime;

import org.antlr.v4.runtime.atn.ATN;

/** A handy class for use with
 *
 *  options {contextSuperClass=org.antlr.v4.runtime.RuleContextWithAltNum;}
 *
 *  that provides a property for the outer alternative number
 *  matched for an internal parse tree node.
 */
public class RuleContextWithAltNum extends ParserRuleContext {
	private int altNumber;

	public RuleContextWithAltNum() {
		altNumber = ATN.INVALID_ALT_NUMBER;
	}

	public RuleContextWithAltNum(ParserRuleContext parent, int invokingStateNumber) {
		super(parent, invokingStateNumber);
	}

	/**
	 * @sharpen.property OuterAlternative
	 */
	@Override
	public int getAltNumber() {
		return altNumber;
	}

	/**
	 * @sharpen.property OuterAlternative
	 */
	@Override
	public void setAltNumber(int altNum) {
		this.altNumber = altNum;
	}
}
