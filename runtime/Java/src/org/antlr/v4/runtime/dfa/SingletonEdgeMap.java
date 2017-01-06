/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.runtime.dfa;

import java.util.Collections;
import java.util.Iterator;
import java.util.Map;
import java.util.NoSuchElementException;
import java.util.Set;

/**
 *
 * @author Sam Harwell
 */
public final class SingletonEdgeMap<T> extends AbstractEdgeMap<T> {

	private final int key;
	private final T value;

	public SingletonEdgeMap(int minIndex, int maxIndex, int key, T value) {
		super(minIndex, maxIndex);
		if (key >= minIndex && key <= maxIndex) {
			this.key = key;
			this.value = value;
		} else {
			this.key = 0;
			this.value = null;
		}
	}

	public int getKey() {
		return key;
	}

	public T getValue() {
		return value;
	}

	@Override
	public int size() {
		return value != null ? 1 : 0;
	}

	@Override
	public boolean isEmpty() {
		return value == null;
	}

	@Override
	public boolean containsKey(int key) {
		return key == this.key && value != null;
	}

	@Override
	public T get(int key) {
		if (key == this.key) {
			return value;
		}

		return null;
	}

	@Override
	public AbstractEdgeMap<T> put(int key, T value) {
		if (key < minIndex || key > maxIndex) {
			return this;
		}

		if (key == this.key || this.value == null) {
			return new SingletonEdgeMap<T>(minIndex, maxIndex, key, value);
		} else if (value != null) {
			AbstractEdgeMap<T> result = new SparseEdgeMap<T>(minIndex, maxIndex);
			result = result.put(this.key, this.value);
			result = result.put(key, value);
			return result;
		} else {
			return this;
		}
	}

	@Override
	public AbstractEdgeMap<T> remove(int key) {
		if (key == this.key && this.value != null) {
			return new EmptyEdgeMap<T>(minIndex, maxIndex);
		}

		return this;
	}

	@Override
	public AbstractEdgeMap<T> clear() {
		if (this.value != null) {
			return new EmptyEdgeMap<T>(minIndex, maxIndex);
		}

		return this;
	}

	@Override
	public Map<Integer, T> toMap() {
		if (isEmpty()) {
			return Collections.emptyMap();
		}

		return Collections.singletonMap(key, value);
	}

	@Override
	public Set<Map.Entry<Integer, T>> entrySet() {
		return new EntrySet();
	}

	private class EntrySet extends AbstractEntrySet {
		@Override
		public Iterator<Map.Entry<Integer, T>> iterator() {
			return new EntryIterator();
		}
	}

	private class EntryIterator implements Iterator<Map.Entry<Integer, T>> {
		private int current;

		@Override
		public boolean hasNext() {
			return current < size();
		}

		@Override
		public Map.Entry<Integer, T> next() {
			if (current >= size()) {
				throw new NoSuchElementException();
			}

			current++;
			return new Map.Entry<Integer, T>() {
				private final int key = SingletonEdgeMap.this.key;
				private final T value = SingletonEdgeMap.this.value;

				@Override
				public Integer getKey() {
					return key;
				}

				@Override
				public T getValue() {
					return value;
				}

				@Override
				public T setValue(T value) {
					throw new UnsupportedOperationException("Not supported yet.");
				}
			};
		}

		@Override
		public void remove() {
			throw new UnsupportedOperationException("Not supported yet.");
		}
	}
}
