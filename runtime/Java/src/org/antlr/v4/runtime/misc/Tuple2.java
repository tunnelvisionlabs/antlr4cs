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
public class Tuple2<T1, T2> {

	private final T1 item1;
	private final T2 item2;

	public Tuple2(T1 item1, T2 item2) {
		this.item1 = item1;
		this.item2 = item2;
	}

	public final T1 getItem1() {
		return item1;
	}

	public final T2 getItem2() {
		return item2;
	}

	@Override
	public boolean equals(Object obj) {
		if (obj == this) {
			return true;
		}
		else if (!(obj instanceof Tuple2<?, ?>)) {
			return false;
		}

		Tuple2<?, ?> other = (Tuple2<?, ?>)obj;
		return Tuple.equals(this.item1, other.item1)
			&& Tuple.equals(this.item2, other.item2);
	}

	@Override
	public int hashCode() {
		int hash = 5;
		hash = 79 * hash + (this.item1 != null ? this.item1.hashCode() : 0);
		hash = 79 * hash + (this.item2 != null ? this.item2.hashCode() : 0);
		return hash;
	}

	@Override
	public String toString() {
		return String.format("(%s, %s)", item1, item2);
	}

}
