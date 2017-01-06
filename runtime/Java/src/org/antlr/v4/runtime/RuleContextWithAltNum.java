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
 *  that provides a backing field / impl for the outer alternative number
 *  matched for an internal parse tree node.
 *
 *  I'm only putting into Java runtime as I'm certain I'm the only one that
 *  will really every use this.
 */
public class RuleContextWithAltNum extends ParserRuleContext {
	private int altNumber;

	public RuleContextWithAltNum() {
		altNumber = ATN.INVALID_ALT_NUMBER;
	}

	public RuleContextWithAltNum(ParserRuleContext parent, int invokingStateNumber) {
		super(parent, invokingStateNumber);
	}

	@Override
	public int getAltNumber() {
		return altNumber;
	}

	@Override
	public void setAltNumber(int altNum) {
		this.altNumber = altNum;
	}
}
