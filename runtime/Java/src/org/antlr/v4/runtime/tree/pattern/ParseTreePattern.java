/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.tree.pattern;

import org.antlr.v4.runtime.misc.NotNull;
import org.antlr.v4.runtime.tree.ParseTree;
import org.antlr.v4.runtime.tree.xpath.XPath;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

/**
 * A pattern like {@code <ID> = <expr>;} converted to a {@link ParseTree} by
 * {@link ParseTreePatternMatcher#compile(String, int)}.
 */
public class ParseTreePattern {
	/**
	 * This is the backing field for {@link #getPatternRuleIndex()}.
	 */
	private final int patternRuleIndex;

	/**
	 * This is the backing field for {@link #getPattern()}.
	 */
	@NotNull
	private final String pattern;

	/**
	 * This is the backing field for {@link #getPatternTree()}.
	 */
	@NotNull
	private final ParseTree patternTree;

	/**
	 * This is the backing field for {@link #getMatcher()}.
	 */
	@NotNull
	private final ParseTreePatternMatcher matcher;

	/**
	 * Construct a new instance of the {@link ParseTreePattern} class.
	 *
	 * @param matcher The {@link ParseTreePatternMatcher} which created this
	 * tree pattern.
	 * @param pattern The tree pattern in concrete syntax form.
	 * @param patternRuleIndex The parser rule which serves as the root of the
	 * tree pattern.
	 * @param patternTree The tree pattern in {@link ParseTree} form.
	 */
	public ParseTreePattern(@NotNull ParseTreePatternMatcher matcher,
							@NotNull String pattern, int patternRuleIndex, @NotNull ParseTree patternTree)
	{
		this.matcher = matcher;
		this.patternRuleIndex = patternRuleIndex;
		this.pattern = pattern;
		this.patternTree = patternTree;
	}

	/**
	 * Match a specific parse tree against this tree pattern.
	 *
	 * @param tree The parse tree to match against this tree pattern.
	 * @return A {@link ParseTreeMatch} object describing the result of the
	 * match operation. The {@link ParseTreeMatch#succeeded()} method can be
	 * used to determine whether or not the match was successful.
	 */
	@NotNull
	public ParseTreeMatch match(@NotNull ParseTree tree) {
		return matcher.match(tree, this);
	}

	/**
	 * Determine whether or not a parse tree matches this tree pattern.
	 *
	 * @param tree The parse tree to match against this tree pattern.
	 * @return {@code true} if {@code tree} is a match for the current tree
	 * pattern; otherwise, {@code false}.
	 */
	public boolean matches(@NotNull ParseTree tree) {
		return matcher.match(tree, this).succeeded();
	}

	/**
	 * Find all nodes using XPath and then try to match those subtrees against
	 * this tree pattern.
	 *
	 * @param tree The {@link ParseTree} to match against this pattern.
	 * @param xpath An expression matching the nodes
	 *
	 * @return A collection of {@link ParseTreeMatch} objects describing the
	 * successful matches. Unsuccessful matches are omitted from the result,
	 * regardless of the reason for the failure.
	 */
	@NotNull
	public List<ParseTreeMatch> findAll(@NotNull ParseTree tree, @NotNull String xpath) {
		Collection<ParseTree> subtrees = XPath.findAll(tree, xpath, matcher.getParser());
		List<ParseTreeMatch> matches = new ArrayList<ParseTreeMatch>();
		for (ParseTree t : subtrees) {
			ParseTreeMatch match = match(t);
			if ( match.succeeded() ) {
				matches.add(match);
			}
		}
		return matches;
	}

	/**
	 * Get the {@link ParseTreePatternMatcher} which created this tree pattern.
	 *
	 * @return The {@link ParseTreePatternMatcher} which created this tree
	 * pattern.
	 */
	@NotNull
	public ParseTreePatternMatcher getMatcher() {
		return matcher;
	}

	/**
	 * Get the tree pattern in concrete syntax form.
	 *
	 * @return The tree pattern in concrete syntax form.
	 */
	@NotNull
	public String getPattern() {
		return pattern;
	}

	/**
	 * Get the parser rule which serves as the outermost rule for the tree
	 * pattern.
	 *
	 * @return The parser rule which serves as the outermost rule for the tree
	 * pattern.
	 */
	public int getPatternRuleIndex() {
		return patternRuleIndex;
	}

	/**
	 * Get the tree pattern as a {@link ParseTree}. The rule and token tags from
	 * the pattern are present in the parse tree as terminal nodes with a symbol
	 * of type {@link RuleTagToken} or {@link TokenTagToken}.
	 *
	 * @return The tree pattern as a {@link ParseTree}.
	 */
	@NotNull
	public ParseTree getPatternTree() {
		return patternTree;
	}
}
