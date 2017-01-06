/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.automata;

import org.antlr.v4.runtime.atn.ATNState;
import org.antlr.v4.runtime.atn.Transition;
import org.antlr.v4.runtime.misc.NotNull;

import java.util.HashSet;
import java.util.Set;

/** A simple visitor that walks everywhere it can go starting from s,
 *  without going into an infinite cycle. Override and implement
 *  visitState() to provide functionality.
 */
public class ATNVisitor {
	public void visit(@NotNull ATNState s) {
		visit_(s, new HashSet<Integer>());
	}

	public void visit_(@NotNull ATNState s, @NotNull Set<Integer> visited) {
		if ( !visited.add(s.stateNumber) ) return;
		visited.add(s.stateNumber);

		visitState(s);
		int n = s.getNumberOfTransitions();
		for (int i=0; i<n; i++) {
			Transition t = s.transition(i);
			visit_(t.target, visited);
		}
	}

	public void visitState(@NotNull ATNState s) { }
}
