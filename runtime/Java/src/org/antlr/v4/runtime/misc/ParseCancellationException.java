/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime.misc;

import org.antlr.v4.runtime.BailErrorStrategy;
import org.antlr.v4.runtime.RecognitionException;

import java.util.concurrent.CancellationException;

/**
 * This exception is thrown to cancel a parsing operation. This exception does
 * not extend {@link RecognitionException}, allowing it to bypass the standard
 * error recovery mechanisms. {@link BailErrorStrategy} throws this exception in
 * response to a parse error.
 *
 * @author Sam Harwell
 */
public class ParseCancellationException extends CancellationException {
	private static final long serialVersionUID = -3529552099366979683L;

	public ParseCancellationException() {
	}

	public ParseCancellationException(String message) {
		super(message);
	}

	public ParseCancellationException(Throwable cause) {
		initCause(cause);
	}

	public ParseCancellationException(String message, Throwable cause) {
		super(message);
		initCause(cause);
	}

}
