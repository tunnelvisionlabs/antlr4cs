/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.runtime.dfa;

import java.util.Collections;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.NoSuchElementException;
import java.util.Set;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.concurrent.atomic.AtomicReferenceArray;

/**
 *
 * @author Sam Harwell
 */
public final class ArrayEdgeMap<T> extends AbstractEdgeMap<T> {

	private final AtomicReferenceArray<T> arrayData;
	private final AtomicInteger size;

	@SuppressWarnings("unchecked")
	public ArrayEdgeMap(int minIndex, int maxIndex) {
		super(minIndex, maxIndex);
		arrayData = new AtomicReferenceArray<T>(maxIndex - minIndex + 1);
		size = new AtomicInteger();
	}

	@Override
	public int size() {
		return size.get();
	}

	@Override
	public boolean isEmpty() {
		return size() == 0;
	}

	@Override
	public boolean containsKey(int key) {
		return get(key) != null;
	}

	@Override
	public T get(int key) {
		if (key < minIndex || key > maxIndex) {
			return null;
		}

		return arrayData.get(key - minIndex);
	}

	@Override
	public ArrayEdgeMap<T> put(int key, T value) {
		if (key >= minIndex && key <= maxIndex) {
			T existing = arrayData.getAndSet(key - minIndex, value);
			if (existing == null && value != null) {
				size.incrementAndGet();
			} else if (existing != null && value == null) {
				size.decrementAndGet();
			}
		}

		return this;
	}

	@Override
	public ArrayEdgeMap<T> remove(int key) {
		return put(key, null);
	}

	@Override
	public ArrayEdgeMap<T> putAll(EdgeMap<? extends T> m) {
		if (m.isEmpty()) {
			return this;
		}

		if (m instanceof ArrayEdgeMap<?>) {
			ArrayEdgeMap<? extends T> other = (ArrayEdgeMap<? extends T>)m;
			int minOverlap = Math.max(minIndex, other.minIndex);
			int maxOverlap = Math.min(maxIndex, other.maxIndex);
			ArrayEdgeMap<T> result = this;
			for (int i = minOverlap; i <= maxOverlap; i++) {
				result = result.put(i, m.get(i));
			}

			return result;
		} else if (m instanceof SingletonEdgeMap<?>) {
			SingletonEdgeMap<? extends T> other = (SingletonEdgeMap<? extends T>)m;
			assert !other.isEmpty();
			return put(other.getKey(), other.getValue());
		} else if (m instanceof SparseEdgeMap<?>) {
			SparseEdgeMap<? extends T> other = (SparseEdgeMap<? extends T>)m;
			synchronized (other) {
				int[] keys = other.getKeys();
				List<? extends T> values = other.getValues();
				ArrayEdgeMap<T> result = this;
				for (int i = 0; i < values.size(); i++) {
					result = result.put(keys[i], values.get(i));
				}
				return result;
			}
		} else {
			throw new UnsupportedOperationException(String.format("EdgeMap of type %s is supported yet.", m.getClass().getName()));
		}
	}

	@Override
	public EmptyEdgeMap<T> clear() {
		return new EmptyEdgeMap<T>(minIndex, maxIndex);
	}

	@Override
	public Map<Integer, T> toMap() {
		if (isEmpty()) {
			return Collections.emptyMap();
		}

		Map<Integer, T> result = new LinkedHashMap<Integer, T>();
		for (int i = 0; i < arrayData.length(); i++) {
			T element = arrayData.get(i);
			if (element == null) {
				continue;
			}

			result.put(i + minIndex, element);
		}

		return result;
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
		private int currentIndex = -1;

		@Override
		public boolean hasNext() {
			return currentIndex < arrayData.length() - 1;
		}

		@Override
		public Map.Entry<Integer, T> next() {
			T element = null;
			while (element == null && currentIndex < arrayData.length() - 1) {
				currentIndex++;
				element = arrayData.get(currentIndex);
			}

			if (element == null) {
				throw new NoSuchElementException();
			}

			currentIndex++;
			final T currentElement = element;
			return new Map.Entry<Integer, T>() {
				private final int key = minIndex + currentIndex - 1;
				private final T value = currentElement;

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
