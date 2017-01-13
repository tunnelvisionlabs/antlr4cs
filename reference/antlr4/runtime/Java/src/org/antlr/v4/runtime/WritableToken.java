/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

package org.antlr.v4.runtime;

public interface WritableToken extends Token {
	/**
	 * @sharpen.property Text
	 */
	public void setText(String text);

	/**
	 * @sharpen.property Type
	 */
	public void setType(int ttype);

	/**
	 * @sharpen.property Line
	 */
	public void setLine(int line);

	/**
	 * @sharpen.property Column
	 */
	public void setCharPositionInLine(int pos);

	/**
	 * @sharpen.property Channel
	 */
	public void setChannel(int channel);

	/**
	 * @sharpen.property TokenIndex
	 */
	public void setTokenIndex(int index);
}
