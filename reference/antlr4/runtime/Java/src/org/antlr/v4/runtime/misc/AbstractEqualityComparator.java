/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.runtime.misc;

/**
 * This abstract base class is provided so performance-critical applications can
 * use virtual- instead of interface-dispatch when calling comparator methods.
 * 
 * @sharpen.ignore
 * @author Sam Harwell
 */
public abstract class AbstractEqualityComparator<T> implements EqualityComparator<T> {

}
