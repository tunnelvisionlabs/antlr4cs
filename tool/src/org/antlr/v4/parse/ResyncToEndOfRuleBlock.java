/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.parse;

/** Used to throw us out of deeply nested element back to end of a rule's
 *  alt list. Note it's not under RecognitionException.
 */
public class ResyncToEndOfRuleBlock extends RuntimeException {
	private static final long serialVersionUID = 6104510295480069276L;
}
