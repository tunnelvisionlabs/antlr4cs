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
