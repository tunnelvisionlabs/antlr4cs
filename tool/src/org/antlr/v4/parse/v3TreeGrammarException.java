/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.parse;

import org.antlr.runtime.Token;
import org.antlr.v4.runtime.misc.ParseCancellationException;

public class v3TreeGrammarException extends ParseCancellationException {
	private static final long serialVersionUID = -8383611621498312969L;

	public Token location;

	public v3TreeGrammarException(Token location) {
		this.location = location;
	}
}
