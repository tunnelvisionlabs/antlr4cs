/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.misc;

/**
 * @sharpen.ignore
 * @author Sam Harwell
 */
public class Tuple3<T1, T2, T3> {

	private final T1 item1;
	private final T2 item2;
	private final T3 item3;

	public Tuple3(T1 item1, T2 item2, T3 item3) {
		this.item1 = item1;
		this.item2 = item2;
		this.item3 = item3;
	}

	public final T1 getItem1() {
		return item1;
	}

	public final T2 getItem2() {
		return item2;
	}

	public final T3 getItem3() {
		return item3;
	}

	@Override
	public boolean equals(Object obj) {
		if (obj == this) {
			return true;
		}
		else if (!(obj instanceof Tuple3<?, ?, ?>)) {
			return false;
		}

		Tuple3<?, ?, ?> other = (Tuple3<?, ?, ?>)obj;
		return Tuple.equals(this.item1, other.item1)
			&& Tuple.equals(this.item2, other.item2)
			&& Tuple.equals(this.item3, other.item3);
	}

	@Override
	public int hashCode() {
		int hash = 5;
		hash = 79 * hash + (this.item1 != null ? this.item1.hashCode() : 0);
		hash = 79 * hash + (this.item2 != null ? this.item2.hashCode() : 0);
		hash = 79 * hash + (this.item3 != null ? this.item3.hashCode() : 0);
		return hash;
	}

	@Override
	public String toString() {
		return String.format("(%s, %s, %s)", item1, item2, item3);
	}

}
