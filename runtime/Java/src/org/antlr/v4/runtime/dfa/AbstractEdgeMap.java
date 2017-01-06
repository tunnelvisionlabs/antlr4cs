/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.runtime.dfa;

import java.util.AbstractSet;
import java.util.Map;

/**
 *
 * @author Sam Harwell
 */
public abstract class AbstractEdgeMap<T> implements EdgeMap<T> {

	protected final int minIndex;
	protected final int maxIndex;

	public AbstractEdgeMap(int minIndex, int maxIndex) {
		// the allowed range (with minIndex and maxIndex inclusive) should be less than 2^32
		assert maxIndex - minIndex + 1 >= 0;
		this.minIndex = minIndex;
		this.maxIndex = maxIndex;
	}

	@Override
	public abstract AbstractEdgeMap<T> put(int key, T value);

	@Override
	public AbstractEdgeMap<T> putAll(EdgeMap<? extends T> m) {
		AbstractEdgeMap<T> result = this;
		for (Map.Entry<Integer, ? extends T> entry : m.entrySet()) {
			result = result.put(entry.getKey(), entry.getValue());
		}

		return result;
	}

	@Override
	public abstract AbstractEdgeMap<T> clear();

	@Override
	public abstract AbstractEdgeMap<T> remove(int key);

	protected abstract class AbstractEntrySet extends AbstractSet<Map.Entry<Integer, T>> {
		@Override
		public boolean contains(Object o) {
			if (!(o instanceof Map.Entry)) {
				return false;
			}

			Map.Entry<?, ?> entry = (Map.Entry<?, ?>)o;
			if (entry.getKey() instanceof Integer) {
				int key = (Integer)entry.getKey();
				Object value = entry.getValue();
				T existing = get(key);
				return value == existing || (existing != null && existing.equals(value));
			}

			return false;
		}

		@Override
		public int size() {
			return AbstractEdgeMap.this.size();
		}
	}

}
