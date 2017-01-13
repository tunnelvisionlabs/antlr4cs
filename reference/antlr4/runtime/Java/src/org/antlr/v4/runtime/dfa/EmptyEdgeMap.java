/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.dfa;

import java.util.Collections;
import java.util.Map;
import java.util.Set;

/**
 * This implementation of {@link AbstractEdgeMap} represents an empty edge map.
 *
 * @author Sam Harwell
 */
public final class EmptyEdgeMap<T> extends AbstractEdgeMap<T> {

	public EmptyEdgeMap(int minIndex, int maxIndex) {
		super(minIndex, maxIndex);
	}

	@Override
	public AbstractEdgeMap<T> put(int key, T value) {
		if (value == null || key < minIndex || key > maxIndex) {
			// remains empty
			return this;
		}

		return new SingletonEdgeMap<T>(minIndex, maxIndex, key, value);
	}

	@Override
	public AbstractEdgeMap<T> clear() {
		return this;
	}

	@Override
	public AbstractEdgeMap<T> remove(int key) {
		return this;
	}

	@Override
	public int size() {
		return 0;
	}

	@Override
	public boolean isEmpty() {
		return true;
	}

	@Override
	public boolean containsKey(int key) {
		return false;
	}

	@Override
	public T get(int key) {
		return null;
	}

	@Override
	public Map<Integer, T> toMap() {
		return Collections.emptyMap();
	}

	@Override
	public Set<Map.Entry<Integer, T>> entrySet() {
		return Collections.<Integer, T>emptyMap().entrySet();
	}
}
