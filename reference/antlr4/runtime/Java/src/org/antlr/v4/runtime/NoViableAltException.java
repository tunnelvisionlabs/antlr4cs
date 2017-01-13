/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.runtime;

import org.antlr.v4.runtime.atn.ATNConfigSet;
import org.antlr.v4.runtime.misc.NotNull;
import org.antlr.v4.runtime.misc.Nullable;

/** Indicates that the parser could not decide which of two or more paths
 *  to take based upon the remaining input. It tracks the starting token
 *  of the offending input and also knows where the parser was
 *  in the various paths when the error. Reported by reportNoViableAlternative()
 */
public class NoViableAltException extends RecognitionException {
	private static final long serialVersionUID = 5096000008992867052L;

	/** Which configurations did we try at input.index() that couldn't match input.LT(1)? */
	@Nullable
	private final ATNConfigSet deadEndConfigs;

	/** The token object at the start index; the input stream might
	 * 	not be buffering tokens so get a reference to it. (At the
	 *  time the error occurred, of course the stream needs to keep a
	 *  buffer all of the tokens but later we might not have access to those.)
	 */
	@NotNull
	private final Token startToken;

	public NoViableAltException(@NotNull Parser recognizer) { // LL(1) error
		this(recognizer,
			 recognizer.getInputStream(),
			 recognizer.getCurrentToken(),
			 recognizer.getCurrentToken(),
			 null,
			 recognizer._ctx);
	}

	public NoViableAltException(@NotNull Recognizer<Token, ?> recognizer,
								@NotNull TokenStream input,
								@NotNull Token startToken,
								@NotNull Token offendingToken,
								@Nullable ATNConfigSet deadEndConfigs,
								@NotNull ParserRuleContext ctx)
	{
		super(recognizer, input, ctx);
		this.deadEndConfigs = deadEndConfigs;
		this.startToken = startToken;
		this.setOffendingToken(offendingToken);
	}

	/**
	 * @sharpen.property StartToken
	 */
	public Token getStartToken() {
		return startToken;
	}

	/**
	 * @sharpen.property DeadEndConfigs
	 */
	@Nullable
	public ATNConfigSet getDeadEndConfigs() {
		return deadEndConfigs;
	}

}
