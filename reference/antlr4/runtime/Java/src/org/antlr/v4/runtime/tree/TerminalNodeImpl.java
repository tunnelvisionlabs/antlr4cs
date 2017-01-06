/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.tree;

import org.antlr.v4.runtime.Parser;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.misc.Interval;

import java.util.List;

public class TerminalNodeImpl implements TerminalNode {
	public Token symbol;
	public RuleNode parent;

	public TerminalNodeImpl(Token symbol) {	this.symbol = symbol;	}

	@Override
	public ParseTree getChild(int i) {return null;}

	@Override
	public Token getSymbol() {return symbol;}

	@Override
	public RuleNode getParent() { return parent; }

	@Override
	public Token getPayload() { return symbol; }

	@Override
	public Interval getSourceInterval() {
		if (symbol != null) {
			int tokenIndex = symbol.getTokenIndex();
			return new Interval(tokenIndex, tokenIndex);
		}

		return Interval.INVALID;
	}

	@Override
	public int getChildCount() { return 0; }

	@Override
	public <T> T accept(ParseTreeVisitor<? extends T> visitor) {
		return visitor.visitTerminal(this);
	}

	@Override
	public String getText() {
		if (symbol != null) {
			return symbol.getText();
		}

		return null;
	}

	@Override
	public String toStringTree(Parser parser) {
		return toString();
	}

	@Override
	public String toString() {
		if (symbol != null) {
			if ( symbol.getType() == Token.EOF ) {
				return "<EOF>";
			}

			return symbol.getText();
		}
		else {
			return "<null>";
		}
	}

	@Override
	public String toStringTree() {
		return toString();
	}
}
