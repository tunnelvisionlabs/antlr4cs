/*
 * Copyright (c) 2012 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD-3-Clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

lexer grammar ActionSplitter;

options { filter=true; }

@header {
package org.antlr.v4.parse;
import org.antlr.v4.tool.*;
import org.antlr.v4.tool.ast.*;
}

@members {
ActionSplitterListener delegate;

public ActionSplitter(CharStream input, ActionSplitterListener delegate) {
    this(input, new RecognizerSharedState());
    this.delegate = delegate;
}

/** force filtering (and return tokens). triggers all above actions. */
public List<Token> getActionTokens() {
    List<Token> chunks = new ArrayList<Token>();
    Token t = nextToken();
    while ( t.getType()!=Token.EOF ) {
        chunks.add(t);
        t = nextToken();
    }
    return chunks;
}

private boolean isIDStartChar(int c) {
	return c == '_' || Character.isLetter(c);
}
}

// ignore comments right away

COMMENT
    :   '/*' ( options {greedy=false;} : . )* '*/' {delegate.text($text);}
    ;

LINE_COMMENT
    : '//' ~('\n'|'\r')* '\r'? '\n' {delegate.text($text);}
    ;

SET_NONLOCAL_ATTR
	:	'$' x=ID '::' y=ID WS? '=' expr=ATTR_VALUE_EXPR ';'
		{
		delegate.setNonLocalAttr($text, $x, $y, $expr);
		}
	;

NONLOCAL_ATTR
	:	'$' x=ID '::' y=ID {delegate.nonLocalAttr($text, $x, $y);}
	;

QUALIFIED_ATTR
	:	'$' x=ID '.' y=ID {input.LA(1)!='('}? {delegate.qualifiedAttr($text, $x, $y);}
	;

SET_ATTR
	:	'$' x=ID WS? '=' expr=ATTR_VALUE_EXPR ';'
		{
		delegate.setAttr($text, $x, $expr);
		}
	;

ATTR
	:	'$' x=ID {delegate.attr($text, $x);}
	;

// Anything else is just random text
TEXT
@init {StringBuilder buf = new StringBuilder();}
@after {delegate.text(buf.toString());}
	:	(	c=~('\\'| '$') {buf.append((char)$c);}
		|	'\\$' {buf.append('$');}
		|	'\\' c=~('$') {buf.append('\\').append((char)$c);}
		|	{!isIDStartChar(input.LA(2))}? => '$' {buf.append('$');}
		)+
	;

fragment
ID  :	('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')*
    ;

/** Don't allow an = as first char to prevent $x == 3; kind of stuff. */
fragment
ATTR_VALUE_EXPR
	:	~'=' (~';')*
	;

fragment
WS	:	(' '|'\t'|'\n'|'\r')+
	;

