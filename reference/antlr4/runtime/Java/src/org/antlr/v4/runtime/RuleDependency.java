/* Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * Declares a dependency upon a grammar rule, along with a set of zero or more
 * dependent rules.
 * <p/>
 * Version numbers within a grammar should be assigned on a monotonically
 * increasing basis to allow for accurate tracking of dependent rules.
 *
 * @author Sam Harwell
 */
@Retention(RetentionPolicy.RUNTIME)
@Target({ElementType.TYPE, ElementType.CONSTRUCTOR, ElementType.METHOD, ElementType.FIELD})
public @interface RuleDependency {

	Class<? extends Recognizer<?, ?>> recognizer();

	int rule();

	int version();

	/**
	 * Specifies the set of grammar rules related to {@link #rule} which the
	 * annotated element depends on. Even when absent from this set, the
	 * annotated element is implicitly dependent upon the explicitly specified
	 * {@link #rule}, which corresponds to the {@link Dependents#SELF} element.
	 * <p/>
	 * By default, the annotated element is dependent upon the specified
	 * {@link #rule} and its {@link Dependents#PARENTS}, i.e. the rule within
	 * one level of context information. The parents are included since the most
	 * frequent assumption about a rule is where it's used in the grammar.
	 */
	Dependents[] dependents() default {Dependents.SELF, Dependents.PARENTS};
}
