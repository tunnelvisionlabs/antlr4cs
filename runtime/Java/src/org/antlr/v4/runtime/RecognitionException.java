/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
package org.antlr.v4.runtime;

import org.antlr.v4.runtime.atn.DecisionState;
import org.antlr.v4.runtime.misc.IntervalSet;
import org.antlr.v4.runtime.misc.Nullable;

/** The root of the ANTLR exception hierarchy. In general, ANTLR tracks just
 *  3 kinds of errors: prediction errors, failed predicate errors, and
 *  mismatched input errors. In each case, the parser knows where it is
 *  in the input, where it is in the ATN, the rule invocation stack,
 *  and what kind of problem occurred.
 */
public class RecognitionException extends RuntimeException {
	private static final long serialVersionUID = -3861826954750022374L;

	/** The {@link Recognizer} where this exception originated. */
	@Nullable
	private final Recognizer<?, ?> recognizer;

	@Nullable
	private final RuleContext ctx;

	@Nullable
	private final IntStream input;

	/**
	 * The current {@link Token} when an error occurred. Since not all streams
	 * support accessing symbols by index, we have to track the {@link Token}
	 * instance itself.
	 */
	private Token offendingToken;

	private int offendingState = -1;

	public RecognitionException(@Nullable Lexer lexer,
								CharStream input)
	{
		this.recognizer = lexer;
		this.input = input;
		this.ctx = null;
	}

	public RecognitionException(@Nullable Recognizer<Token, ?> recognizer,
								@Nullable IntStream input,
								@Nullable ParserRuleContext ctx)
	{
		this.recognizer = recognizer;
		this.input = input;
		this.ctx = ctx;
		if ( recognizer!=null ) this.offendingState = recognizer.getState();
	}

	public RecognitionException(String message,
								@Nullable Recognizer<Token, ?> recognizer,
								@Nullable IntStream input,
								@Nullable ParserRuleContext ctx)
	{
		super(message);
		this.recognizer = recognizer;
		this.input = input;
		this.ctx = ctx;
		if ( recognizer!=null ) this.offendingState = recognizer.getState();
	}

	/**
	 * Get the ATN state number the parser was in at the time the error
	 * occurred. For {@link NoViableAltException} and
	 * {@link LexerNoViableAltException} exceptions, this is the
	 * {@link DecisionState} number. For others, it is the state whose outgoing
	 * edge we couldn't match.
	 *
	 * <p>If the state number is not known, this method returns -1.</p>
	 */
	public int getOffendingState() {
		return offendingState;
	}

	protected final void setOffendingState(int offendingState) {
		this.offendingState = offendingState;
	}

	/**
	 * Gets the set of input symbols which could potentially follow the
	 * previously matched symbol at the time this exception was thrown.
	 *
	 * <p>If the set of expected tokens is not known and could not be computed,
	 * this method returns {@code null}.</p>
	 *
	 * @return The set of token types that could potentially follow the current
	 * state in the ATN, or {@code null} if the information is not available.
	 */
	@Nullable
	public IntervalSet getExpectedTokens() {
		if (recognizer != null) {
			return recognizer.getATN().getExpectedTokens(offendingState, ctx);
		}

		return null;
	}

	/**
	 * Gets the {@link RuleContext} at the time this exception was thrown.
	 *
	 * <p>If the context is not available, this method returns {@code null}.</p>
	 *
	 * @return The {@link RuleContext} at the time this exception was thrown.
	 * If the context is not available, this method returns {@code null}.
	 */
	@Nullable
	public RuleContext getContext() {
		return ctx;
	}

	/**
	 * Gets the input stream which is the symbol source for the recognizer where
	 * this exception was thrown.
	 *
	 * <p>If the input stream is not available, this method returns {@code null}.</p>
	 *
	 * @return The input stream which is the symbol source for the recognizer
	 * where this exception was thrown, or {@code null} if the stream is not
	 * available.
	 */
	@Nullable
	public IntStream getInputStream() {
		return input;
	}

	@Nullable
	public Token getOffendingToken() {
		return offendingToken;
	}

	protected final <Symbol extends Token> void setOffendingToken(Recognizer<Symbol, ?> recognizer, @Nullable Symbol offendingToken) {
		if (recognizer == this.recognizer) {
			this.offendingToken = offendingToken;
		}
	}

	/**
	 * Gets the {@link Recognizer} where this exception occurred.
	 *
	 * <p>If the recognizer is not available, this method returns {@code null}.</p>
	 *
	 * @return The recognizer where this exception occurred, or {@code null} if
	 * the recognizer is not available.
	 */
	@Nullable
	public Recognizer<?, ?> getRecognizer() {
		return recognizer;
	}

	@SuppressWarnings("unchecked") // safe
	public <T> T getOffendingToken(Recognizer<T, ?> recognizer) {
		return this.recognizer == recognizer ? (T)offendingToken : null;
	}
}
