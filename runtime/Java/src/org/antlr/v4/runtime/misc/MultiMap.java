/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.misc;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;

public class MultiMap<K, V> extends LinkedHashMap<K, List<V>> {
	private static final long serialVersionUID = -4956746660057462312L;

	public void map(K key, V value) {
		List<V> elementsForKey = get(key);
		if ( elementsForKey==null ) {
			elementsForKey = new ArrayList<V>();
			super.put(key, elementsForKey);
		}
		elementsForKey.add(value);
	}

	public List<Tuple2<K, V>> getPairs() {
		List<Tuple2<K, V>> pairs = new ArrayList<Tuple2<K, V>>();
		for (K key : keySet()) {
			for (V value : get(key)) {
				pairs.add(Tuple.create(key, value));
			}
		}
		return pairs;
	}
}
