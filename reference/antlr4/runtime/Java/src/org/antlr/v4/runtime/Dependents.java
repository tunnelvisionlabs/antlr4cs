/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime;

/**
 *
 * @author Sam Harwell
 */
public enum Dependents {

	/**
	 * The element is dependent upon the specified rule.
	 */
	SELF,
	/**
	 * The element is dependent upon the set of the specified rule's parents
	 * (rules which directly reference it).
	 */
	PARENTS,
	/**
	 * The element is dependent upon the set of the specified rule's children
	 * (rules which it directly references).
	 */
	CHILDREN,
	/**
	 * The element is dependent upon the set of the specified rule's ancestors
	 * (the transitive closure of {@link #PARENTS} rules).
	 */
	ANCESTORS,
	/**
	 * The element is dependent upon the set of the specified rule's descendants
	 * (the transitive closure of {@link #CHILDREN} rules).
	 */
	DESCENDANTS,
	/**
	 * The element is dependent upon the set of the specified rule's siblings
	 * (the union of {@link #CHILDREN} of its {@link #PARENTS}).
	 */
	SIBLINGS,
	/**
	 * The element is dependent upon the set of the specified rule's preceeding
	 * siblings (the union of {@link #CHILDREN} of its {@link #PARENTS} which
	 * appear before a reference to the rule).
	 */
	PRECEEDING_SIBLINGS,
	/**
	 * The element is dependent upon the set of the specified rule's following
	 * siblings (the union of {@link #CHILDREN} of its {@link #PARENTS} which
	 * appear after a reference to the rule).
	 */
	FOLLOWING_SIBLINGS,
	/**
	 * The element is dependent upon the set of the specified rule's preceeding
	 * elements (rules which might end before the start of the specified rule
	 * while parsing). This is calculated by taking the
	 * {@link #PRECEEDING_SIBLINGS} of the rule and each of its
	 * {@link #ANCESTORS}, along with the {@link #DESCENDANTS} of those
	 * elements.
	 */
	PRECEEDING,
	/**
	 * The element is dependent upon the set of the specified rule's following
	 * elements (rules which might start after the end of the specified rule
	 * while parsing). This is calculated by taking the
	 * {@link #FOLLOWING_SIBLINGS} of the rule and each of its
	 * {@link #ANCESTORS}, along with the {@link #DESCENDANTS} of those
	 * elements.
	 */
	FOLLOWING,
}
