/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.runtime;

import org.antlr.v4.runtime.misc.Interval;
import org.antlr.v4.runtime.misc.NotNull;
import org.antlr.v4.runtime.misc.Nullable;
import org.antlr.v4.runtime.misc.Tuple;
import org.antlr.v4.runtime.misc.Tuple2;

import java.io.Serializable;

public class CommonToken implements WritableToken, Serializable {
	private static final long serialVersionUID = -6708843461296520577L;

	/**
	 * An empty {@link Tuple2} which is used as the default value of
	 * {@link #source} for tokens that do not have a source.
	 */
	protected static final Tuple2<TokenSource, CharStream> EMPTY_SOURCE =
		Tuple.<TokenSource, CharStream>create(null, null);

	/**
	 * This is the backing field for {@link #getType} and {@link #setType}.
	 */
	protected int type;
	/**
	 * This is the backing field for {@link #getLine} and {@link #setLine}.
	 */
	protected int line;
	/**
	 * This is the backing field for {@link #getCharPositionInLine} and
	 * {@link #setCharPositionInLine}.
	 */
	protected int charPositionInLine = -1; // set to invalid position
	/**
	 * This is the backing field for {@link #getChannel} and
	 * {@link #setChannel}.
	 */
	protected int channel=DEFAULT_CHANNEL;
	/**
	 * This is the backing field for {@link #getTokenSource} and
	 * {@link #getInputStream}.
	 *
	 * <p>
	 * These properties share a field to reduce the memory footprint of
	 * {@link CommonToken}. Tokens created by a {@link CommonTokenFactory} from
	 * the same source and input stream share a reference to the same
	 * {@link Tuple2} containing these values.</p>
	 */
	@NotNull
	protected Tuple2<? extends TokenSource, CharStream> source;

	/**
	 * This is the backing field for {@link #getText} when the token text is
	 * explicitly set in the constructor or via {@link #setText}.
	 *
	 * @see #getText()
	 */
	protected String text;

	/**
	 * This is the backing field for {@link #getTokenIndex} and
	 * {@link #setTokenIndex}.
	 */
	protected int index = -1;

	/**
	 * This is the backing field for {@link #getStartIndex} and
	 * {@link #setStartIndex}.
	 */
	protected int start;

	/**
	 * This is the backing field for {@link #getStopIndex} and
	 * {@link #setStopIndex}.
	 */
	protected int stop;

	/**
	 * Constructs a new {@link CommonToken} with the specified token type.
	 *
	 * @param type The token type.
	 */
	public CommonToken(int type) {
		this.type = type;
		this.source = EMPTY_SOURCE;
	}

	public CommonToken(@NotNull Tuple2<? extends TokenSource, CharStream> source, int type, int channel, int start, int stop) {
		this.source = source;
		this.type = type;
		this.channel = channel;
		this.start = start;
		this.stop = stop;
		if (source.getItem1() != null) {
			this.line = source.getItem1().getLine();
			this.charPositionInLine = source.getItem1().getCharPositionInLine();
		}
	}

	/**
	 * Constructs a new {@link CommonToken} with the specified token type and
	 * text.
	 *
	 * @param type The token type.
	 * @param text The text of the token.
	 */
	public CommonToken(int type, String text) {
		this.type = type;
		this.channel = DEFAULT_CHANNEL;
		this.text = text;
		this.source = EMPTY_SOURCE;
	}

	/**
	 * Constructs a new {@link CommonToken} as a copy of another {@link Token}.
	 *
	 * <p>
	 * If {@code oldToken} is also a {@link CommonToken} instance, the newly
	 * constructed token will share a reference to the {@link #text} field and
	 * the {@link Tuple2} stored in {@link #source}. Otherwise, {@link #text} will
	 * be assigned the result of calling {@link #getText}, and {@link #source}
	 * will be constructed from the result of {@link Token#getTokenSource} and
	 * {@link Token#getInputStream}.</p>
	 *
	 * @param oldToken The token to copy.
	 */
	public CommonToken(@NotNull Token oldToken) {
		type = oldToken.getType();
		line = oldToken.getLine();
		index = oldToken.getTokenIndex();
		charPositionInLine = oldToken.getCharPositionInLine();
		channel = oldToken.getChannel();
		start = oldToken.getStartIndex();
		stop = oldToken.getStopIndex();

		if (oldToken instanceof CommonToken) {
			text = ((CommonToken)oldToken).text;
			source = ((CommonToken)oldToken).source;
		}
		else {
			text = oldToken.getText();
			source = Tuple.create(oldToken.getTokenSource(), oldToken.getInputStream());
		}
	}

	@Override
	public int getType() {
		return type;
	}

	@Override
	public void setLine(int line) {
		this.line = line;
	}

	@Override
	public String getText() {
		if ( text!=null ) {
			return text;
		}

		CharStream input = getInputStream();
		if ( input==null ) return null;
		int n = input.size();
		if ( start<n && stop<n) {
			return input.getText(Interval.of(start,stop));
		}
		else {
			return "<EOF>";
		}
	}

	/**
	 * Explicitly set the text for this token. If {code text} is not
	 * {@code null}, then {@link #getText} will return this value rather than
	 * extracting the text from the input.
	 *
	 * @param text The explicit text of the token, or {@code null} if the text
	 * should be obtained from the input along with the start and stop indexes
	 * of the token.
	 */
	@Override
	public void setText(String text) {
		this.text = text;
	}

	@Override
	public int getLine() {
		return line;
	}

	@Override
	public int getCharPositionInLine() {
		return charPositionInLine;
	}

	@Override
	public void setCharPositionInLine(int charPositionInLine) {
		this.charPositionInLine = charPositionInLine;
	}

	@Override
	public int getChannel() {
		return channel;
	}

	@Override
	public void setChannel(int channel) {
		this.channel = channel;
	}

	@Override
	public void setType(int type) {
		this.type = type;
	}

	@Override
	public int getStartIndex() {
		return start;
	}

	/**
	 * @sharpen.property StartIndex
	 */
	public void setStartIndex(int start) {
		this.start = start;
	}

	@Override
	public int getStopIndex() {
		return stop;
	}

	/**
	 * @sharpen.property StopIndex
	 */
	public void setStopIndex(int stop) {
		this.stop = stop;
	}

	@Override
	public int getTokenIndex() {
		return index;
	}

	@Override
	public void setTokenIndex(int index) {
		this.index = index;
	}

	@Override
	public TokenSource getTokenSource() {
		return source.getItem1();
	}

	@Override
	public CharStream getInputStream() {
		return source.getItem2();
	}

	@Override
	public String toString() {
		return toString(null);
	}

	public String toString(@Nullable Recognizer<?, ?> r) {
		String channelStr = "";
		if ( channel>0 ) {
			channelStr=",channel="+channel;
		}
		String txt = getText();
		if ( txt!=null ) {
			txt = txt.replace("\n","\\n");
			txt = txt.replace("\r","\\r");
			txt = txt.replace("\t","\\t");
		}
		else {
			txt = "<no text>";
		}

		String typeString = String.valueOf(type);
		if ( r!=null ) {
			typeString = r.getVocabulary().getDisplayName(type);
		}

		return "[@"+getTokenIndex()+","+start+":"+stop+"='"+txt+"',<"+typeString+">"+channelStr+","+line+":"+getCharPositionInLine()+"]";
	}
}
