/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.misc;

import java.lang.annotation.Documented;
import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * This annotation marks a field, parameter, local variable, or method (return
 * value) as never being {@code null}. The specific semantics implied by this
 * annotation depend on the kind of element the annotation is applied to.
 *
 * <ul>
 * <li><strong>Field or Local Variable:</strong> Code reading the field or local
 * variable may assume that the value is never {@code null}. Code writing to the
 * field or local variable should ensure that a {@code null} reference is never
 * written.</li>
 * <li><strong>Parameter:</strong> Code calling the method should never pass
 * {@code null} for this parameter. The implementation may assume that the value
 * is never {@code null}, and the behavior of the method if the parameter is
 * {@code null} is undefined. Overriding methods may optionally use the
 * {@link Nullable} annotation instead of this annotation for the parameter,
 * indicating that the overriding method provides additional code to handle a
 * {@code null} reference passed for the parameter.</li>
 * <li><strong>Method (Return Value):</strong> Code calling the method may
 * assume that the result of the method is never {@code null}. The
 * implementation of the method should ensure that a {@code null} reference is
 * never returned.</li>
 * </ul>
 *
 * <p>
 * The {@link NullUsageProcessor} annotation processor validates certain usage
 * scenarios for this annotation, with compile-time errors or warnings reported
 * for misuse. For detailed information about the supported analysis, see the
 * documentation for {@link NullUsageProcessor}.</p>
 */
@Documented
@Retention(RetentionPolicy.CLASS)
@Target({ElementType.FIELD, ElementType.METHOD, ElementType.PARAMETER, ElementType.LOCAL_VARIABLE})
public @interface NotNull {
}
