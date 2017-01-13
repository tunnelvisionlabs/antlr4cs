/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime;

import org.antlr.v4.runtime.misc.NotNull;
import org.antlr.v4.runtime.misc.Tuple2;

/** The default mechanism for creating tokens. It's used by default in Lexer and
 *  the error handling strategy (to create missing tokens).  Notifying the parser
 *  of a new factory means that it notifies it's token source and error strategy.
 */
public interface TokenFactory {
	/** This is the method used to create tokens in the lexer and in the
	 *  error handling strategy. If text!=null, than the start and stop positions
	 *  are wiped to -1 in the text override is set in the CommonToken.
	 */
	@NotNull
	Token create(@NotNull Tuple2<? extends TokenSource, CharStream> source, int type, String text,
				  int channel, int start, int stop,
				  int line, int charPositionInLine);

	/** Generically useful */
	@NotNull
	Token create(int type, String text);
}
