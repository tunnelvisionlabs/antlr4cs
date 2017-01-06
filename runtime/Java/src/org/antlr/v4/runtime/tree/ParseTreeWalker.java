/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.tree;

import org.antlr.v4.runtime.ParserRuleContext;
import org.antlr.v4.runtime.RuleContext;
import org.antlr.v4.runtime.misc.IntegerStack;

import java.util.ArrayDeque;
import java.util.Deque;

public class ParseTreeWalker {
	public static final ParseTreeWalker DEFAULT = new ParseTreeWalker();

	public void walk(ParseTreeListener listener, ParseTree t) {
		final Deque<ParseTree> nodeStack = new ArrayDeque<ParseTree>();
		final IntegerStack indexStack = new IntegerStack();

		ParseTree currentNode = t;
		int currentIndex = 0;

		while (currentNode != null) {
			// pre-order visit
			if (currentNode instanceof ErrorNode) {
				listener.visitErrorNode((ErrorNode)currentNode);
			} else if (currentNode instanceof TerminalNode) {
				listener.visitTerminal((TerminalNode)currentNode);
			} else {
				final RuleNode r = (RuleNode)currentNode;
				enterRule(listener, r);
			}

			// Move down to first child, if exists
			if (currentNode.getChildCount() > 0) {
				nodeStack.push(currentNode);
				indexStack.push(currentIndex);
				currentIndex = 0;
				currentNode = currentNode.getChild(0);
				continue;
			}

			// No child nodes, so walk tree
			do {

				// post-order visit
				if (currentNode instanceof RuleNode) {
					exitRule(listener, (RuleNode)currentNode);
				}

				// No parent, so no siblings
				if (nodeStack.isEmpty()) {
					currentNode = null;
					currentIndex = 0;
					break;
				}

				// Move to next sibling if possible
				currentNode = nodeStack.peek().getChild(++currentIndex);
				if (currentNode != null) {
					break;
				}

				// No next sibling, so move up
				currentNode = nodeStack.pop();
				currentIndex = indexStack.pop();

			} while (currentNode != null);
		}
	}

	/**
	 * The discovery of a rule node, involves sending two events: the generic
	 * {@link ParseTreeListener#enterEveryRule} and a
	 * {@link RuleContext}-specific event. First we trigger the generic and then
	 * the rule specific. We to them in reverse order upon finishing the node.
	 */
	protected void enterRule(ParseTreeListener listener, RuleNode r) {
		ParserRuleContext ctx = (ParserRuleContext)r.getRuleContext();
		listener.enterEveryRule(ctx);
		ctx.enterRule(listener);
	}

	protected void exitRule(ParseTreeListener listener, RuleNode r) {
		ParserRuleContext ctx = (ParserRuleContext)r.getRuleContext();
		ctx.exitRule(listener);
		listener.exitEveryRule(ctx);
	}
}
