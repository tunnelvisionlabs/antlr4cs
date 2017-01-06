/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.runtime;

import org.antlr.v4.runtime.misc.NotNull;

/** This signifies any kind of mismatched input exceptions such as
 *  when the current input does not match the expected token.
 */
public class InputMismatchException extends RecognitionException {
	private static final long serialVersionUID = 1532568338707443067L;

	public InputMismatchException(@NotNull Parser recognizer) {
		super(recognizer, recognizer.getInputStream(), recognizer._ctx);
		this.setOffendingToken(recognizer, recognizer.getCurrentToken());
	}
}
