/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.misc;

/**
 *
 * @author Sam Harwell
 */
public final class Tuple {

	public static <T1, T2> Tuple2<T1, T2> create(T1 item1, T2 item2) {
		return new Tuple2<T1, T2>(item1, item2);
	}

	public static <T1, T2, T3> Tuple3<T1, T2, T3> create(T1 item1, T2 item2, T3 item3) {
		return new Tuple3<T1, T2, T3>(item1, item2, item3);
	}

	/*package*/ static boolean equals(Object x, Object y) {
		return x == y || (x != null && x.equals(y));
	}

	// static utility class
	private Tuple() {}

}
